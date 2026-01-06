using System.Collections.Generic;
using UnityEngine;
using MDPro3.Servant;

namespace MDPro3.ControllerSupport
{
    /// <summary>
    /// Provides controller support specifically for the deck builder/editor,
    /// including card list navigation, deck manipulation, and quick actions.
    /// </summary>
    public class ControllerDeckBuilderSupport : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] private float cardNavigationCooldown = 0.1f;
        [SerializeField] private int cardsPerRow = 8;

        // Navigation state
        private int currentCardIndex;
        private int totalCards;
        private bool inDeckView;
        private bool inCollectionView;
        private float lastNavigationTime;

        // Current deck region
        public enum DeckRegion
        {
            MainDeck,
            ExtraDeck,
            SideDeck,
            CardCollection,
            SearchResults,
            RelatedCards
        }
        private DeckRegion currentRegion = DeckRegion.CardCollection;

        private static ControllerDeckBuilderSupport instance;
        public static ControllerDeckBuilderSupport Instance => instance;

        // Events
        public delegate void CardIndexChangedHandler(int index, DeckRegion region);
        public delegate void RegionChangedHandler(DeckRegion newRegion);
        
        public event CardIndexChangedHandler OnCardIndexChanged;
        public event RegionChangedHandler OnRegionChanged;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            ControllerManager.OnGameActionTriggered += OnGameAction;
            ControllerManager.IsDeckBuilderMode = true;
        }

        private void OnDisable()
        {
            ControllerManager.OnGameActionTriggered -= OnGameAction;
            ControllerManager.IsDeckBuilderMode = false;
        }

        private void OnGameAction(GameAction action)
        {
            if (!enabled) return;

            switch (action)
            {
                case GameAction.NavigateUp:
                    NavigateUp();
                    break;
                case GameAction.NavigateDown:
                    NavigateDown();
                    break;
                case GameAction.NavigateLeft:
                    NavigateLeft();
                    break;
                case GameAction.NavigateRight:
                    NavigateRight();
                    break;
                case GameAction.Confirm:
                    ConfirmAction();
                    break;
                case GameAction.Cancel:
                    CancelAction();
                    break;
                case GameAction.AddCard:
                    AddSelectedCard();
                    break;
                case GameAction.RemoveCard:
                    RemoveSelectedCard();
                    break;
                case GameAction.SearchCard:
                    OpenSearch();
                    break;
                case GameAction.SortDeck:
                    SortCurrentDeck();
                    break;
                case GameAction.PreviousTab:
                    PreviousTab();
                    break;
                case GameAction.NextTab:
                    NextTab();
                    break;
                case GameAction.ShowCardInfo:
                    ShowCardDetails();
                    break;
            }
        }

        /// <summary>
        /// Navigate up in the card grid
        /// </summary>
        public void NavigateUp()
        {
            if (Time.unscaledTime - lastNavigationTime < cardNavigationCooldown) return;
            lastNavigationTime = Time.unscaledTime;

            switch (currentRegion)
            {
                case DeckRegion.CardCollection:
                case DeckRegion.SearchResults:
                case DeckRegion.RelatedCards:
                    // Move up one row
                    if (currentCardIndex >= cardsPerRow)
                    {
                        currentCardIndex -= cardsPerRow;
                        OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
                        ControllerManager.Instance?.HapticNavigate();
                    }
                    else
                    {
                        // Switch to deck view
                        SwitchToDeckView();
                    }
                    break;
                    
                case DeckRegion.MainDeck:
                case DeckRegion.ExtraDeck:
                case DeckRegion.SideDeck:
                    // Move up within deck region or switch regions
                    NavigateUpInDeck();
                    break;
            }
        }

        /// <summary>
        /// Navigate down in the card grid
        /// </summary>
        public void NavigateDown()
        {
            if (Time.unscaledTime - lastNavigationTime < cardNavigationCooldown) return;
            lastNavigationTime = Time.unscaledTime;

            switch (currentRegion)
            {
                case DeckRegion.CardCollection:
                case DeckRegion.SearchResults:
                case DeckRegion.RelatedCards:
                    // Move down one row
                    var nextIndex = currentCardIndex + cardsPerRow;
                    if (nextIndex < totalCards)
                    {
                        currentCardIndex = nextIndex;
                        OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
                    }
                    ControllerManager.Instance?.HapticNavigate();
                    break;
                    
                case DeckRegion.MainDeck:
                case DeckRegion.ExtraDeck:
                case DeckRegion.SideDeck:
                    // Move down within deck region or switch to collection
                    NavigateDownInDeck();
                    break;
            }
        }

        /// <summary>
        /// Navigate left in the current view
        /// </summary>
        public void NavigateLeft()
        {
            if (Time.unscaledTime - lastNavigationTime < cardNavigationCooldown) return;
            lastNavigationTime = Time.unscaledTime;

            if (currentCardIndex > 0)
            {
                currentCardIndex--;
                OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
                ControllerManager.Instance?.HapticNavigate();
            }
        }

        /// <summary>
        /// Navigate right in the current view
        /// </summary>
        public void NavigateRight()
        {
            if (Time.unscaledTime - lastNavigationTime < cardNavigationCooldown) return;
            lastNavigationTime = Time.unscaledTime;

            if (currentCardIndex < totalCards - 1)
            {
                currentCardIndex++;
                OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
                ControllerManager.Instance?.HapticNavigate();
            }
        }

        private void NavigateUpInDeck()
        {
            var row = currentCardIndex / cardsPerRow;
            
            if (row > 0)
            {
                // Move up within same deck region
                currentCardIndex -= cardsPerRow;
                OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
            }
            else
            {
                // Switch to previous deck region
                switch (currentRegion)
                {
                    case DeckRegion.SideDeck:
                        SwitchToRegion(DeckRegion.ExtraDeck);
                        break;
                    case DeckRegion.ExtraDeck:
                        SwitchToRegion(DeckRegion.MainDeck);
                        break;
                    case DeckRegion.MainDeck:
                        // Already at top
                        break;
                }
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        private void NavigateDownInDeck()
        {
            var row = currentCardIndex / cardsPerRow;
            var maxRows = (totalCards - 1) / cardsPerRow;
            
            if (row < maxRows)
            {
                // Move down within same deck region
                var nextIndex = currentCardIndex + cardsPerRow;
                currentCardIndex = Mathf.Min(nextIndex, totalCards - 1);
                OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
            }
            else
            {
                // Switch to next deck region or collection
                switch (currentRegion)
                {
                    case DeckRegion.MainDeck:
                        SwitchToRegion(DeckRegion.ExtraDeck);
                        break;
                    case DeckRegion.ExtraDeck:
                        SwitchToRegion(DeckRegion.SideDeck);
                        break;
                    case DeckRegion.SideDeck:
                        SwitchToCollectionView();
                        break;
                }
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Confirm action on current selection
        /// </summary>
        public void ConfirmAction()
        {
            switch (currentRegion)
            {
                case DeckRegion.CardCollection:
                case DeckRegion.SearchResults:
                case DeckRegion.RelatedCards:
                    // Add card to deck
                    AddSelectedCard();
                    break;
                    
                case DeckRegion.MainDeck:
                case DeckRegion.ExtraDeck:
                case DeckRegion.SideDeck:
                    // Show card action menu
                    ShowCardActionMenu();
                    break;
            }
            ControllerManager.Instance?.HapticConfirm();
        }

        /// <summary>
        /// Cancel current action
        /// </summary>
        public void CancelAction()
        {
            if (inDeckView)
            {
                SwitchToCollectionView();
            }
            ControllerManager.Instance?.HapticCancel();
        }

        /// <summary>
        /// Add the currently selected card to the deck
        /// </summary>
        public void AddSelectedCard()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Trigger add card through deck editor UI
            // This interfaces with the existing DeckEditorUI system
        }

        /// <summary>
        /// Remove the currently selected card from the deck
        /// </summary>
        public void RemoveSelectedCard()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Trigger remove card through deck editor UI
        }

        /// <summary>
        /// Open the card search interface
        /// </summary>
        public void OpenSearch()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Activate search input field
        }

        /// <summary>
        /// Sort the current deck region
        /// </summary>
        public void SortCurrentDeck()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Trigger deck sort
        }

        /// <summary>
        /// Show detailed card information
        /// </summary>
        public void ShowCardDetails()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Show card detail view
        }

        /// <summary>
        /// Show action menu for card in deck
        /// </summary>
        public void ShowCardActionMenu()
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor?.servantUI == null) return;

            // Show card action menu (move to side, remove, etc.)
        }

        /// <summary>
        /// Switch to previous tab (LB)
        /// </summary>
        public void PreviousTab()
        {
            switch (currentRegion)
            {
                case DeckRegion.CardCollection:
                    // Switch filter tab
                    break;
                case DeckRegion.MainDeck:
                    // Already at first deck section
                    break;
                case DeckRegion.ExtraDeck:
                    SwitchToRegion(DeckRegion.MainDeck);
                    break;
                case DeckRegion.SideDeck:
                    SwitchToRegion(DeckRegion.ExtraDeck);
                    break;
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Switch to next tab (RB)
        /// </summary>
        public void NextTab()
        {
            switch (currentRegion)
            {
                case DeckRegion.CardCollection:
                    // Switch filter tab
                    break;
                case DeckRegion.MainDeck:
                    SwitchToRegion(DeckRegion.ExtraDeck);
                    break;
                case DeckRegion.ExtraDeck:
                    SwitchToRegion(DeckRegion.SideDeck);
                    break;
                case DeckRegion.SideDeck:
                    // Already at last deck section
                    break;
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Switch to deck view mode
        /// </summary>
        public void SwitchToDeckView()
        {
            inDeckView = true;
            inCollectionView = false;
            SwitchToRegion(DeckRegion.MainDeck);
        }

        /// <summary>
        /// Switch to collection view mode
        /// </summary>
        public void SwitchToCollectionView()
        {
            inDeckView = false;
            inCollectionView = true;
            SwitchToRegion(DeckRegion.CardCollection);
        }

        /// <summary>
        /// Switch to a specific region
        /// </summary>
        public void SwitchToRegion(DeckRegion region)
        {
            currentRegion = region;
            currentCardIndex = 0;
            totalCards = GetTotalCardsForRegion(region);
            OnRegionChanged?.Invoke(region);
            OnCardIndexChanged?.Invoke(currentCardIndex, currentRegion);
        }

        private int GetTotalCardsForRegion(DeckRegion region)
        {
            var deckEditor = Program.instance?.deckEditor;
            if (deckEditor == null || DeckEditor.Deck == null) return 0;

            return region switch
            {
                DeckRegion.MainDeck => DeckEditor.Deck.Main.Count,
                DeckRegion.ExtraDeck => DeckEditor.Deck.Extra.Count,
                DeckRegion.SideDeck => DeckEditor.Deck.Side.Count,
                DeckRegion.CardCollection => GetCollectionCardCount(),
                _ => 0
            };
        }

        private int GetCollectionCardCount()
        {
            // Get count from collection view
            // This depends on current filters
            return 0;
        }

        /// <summary>
        /// Update total card count (called when cards change)
        /// </summary>
        public void UpdateCardCount(int count)
        {
            totalCards = count;
            if (currentCardIndex >= totalCards)
            {
                currentCardIndex = Mathf.Max(0, totalCards - 1);
            }
        }

        /// <summary>
        /// Set current card index (called by UI)
        /// </summary>
        public void SetCurrentIndex(int index, DeckRegion region)
        {
            currentCardIndex = index;
            currentRegion = region;
        }

        /// <summary>
        /// Reset navigation state
        /// </summary>
        public void ResetNavigation()
        {
            currentCardIndex = 0;
            currentRegion = DeckRegion.CardCollection;
            inDeckView = false;
            inCollectionView = true;
        }

        /// <summary>
        /// Get current selection info
        /// </summary>
        public (int index, DeckRegion region) GetCurrentSelection()
        {
            return (currentCardIndex, currentRegion);
        }
    }
}
