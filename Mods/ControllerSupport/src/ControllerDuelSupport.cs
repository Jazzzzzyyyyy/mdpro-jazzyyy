using System.Collections.Generic;
using UnityEngine;
using MDPro3.Duel;
using MDPro3.Servant;

namespace MDPro3.ControllerSupport
{
    /// <summary>
    /// Provides controller support specifically for duel gameplay,
    /// including card selection, zone navigation, and action confirmation.
    /// </summary>
    public class ControllerDuelSupport : MonoBehaviour
    {
        [Header("Zone Navigation")]
        [SerializeField] private float zoneNavigationSpeed = 0.2f;
        [SerializeField] private float cardSelectCooldown = 0.15f;

        // Zone grid representation for navigation
        // Row 0: My spell/trap zones (5 zones)
        // Row 1: My monster zones (5 zones)
        // Row 2: Extra monster zones (2 zones)
        // Row 3: Opponent monster zones (5 zones)
        // Row 4: Opponent spell/trap zones (5 zones)
        private const int ZONE_ROWS = 5;
        private const int ZONE_COLS = 5;

        private int currentZoneRow;
        private int currentZoneCol;
        private int currentHandIndex;
        private float lastSelectTime;

        private bool inHandNavigation;
        private bool inZoneNavigation;
        private GameCard selectedCard;
        private List<GameCard> selectableCards = new List<GameCard>();

        private static ControllerDuelSupport instance;
        public static ControllerDuelSupport Instance => instance;

        // Events for UI feedback
        public delegate void ZoneChangedHandler(int row, int col);
        public delegate void CardSelectedHandler(GameCard card);
        public delegate void HandIndexChangedHandler(int index);
        
        public event ZoneChangedHandler OnZoneChanged;
        public event CardSelectedHandler OnCardSelected;
        public event HandIndexChangedHandler OnHandIndexChanged;

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            ControllerManager.OnGameActionTriggered += OnGameAction;
            ControllerManager.IsDuelMode = true;
        }

        private void OnDisable()
        {
            ControllerManager.OnGameActionTriggered -= OnGameAction;
            ControllerManager.IsDuelMode = false;
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
                case GameAction.ViewGraveyard:
                    ViewGraveyard();
                    break;
                case GameAction.ViewBanished:
                    ViewBanished();
                    break;
                case GameAction.ViewExtraDeck:
                    ViewExtraDeck();
                    break;
                case GameAction.ViewHand:
                    ToggleHandNavigation();
                    break;
                case GameAction.ShowCardInfo:
                    ShowCardInfo();
                    break;
                case GameAction.EndPhase:
                    TryEndPhase();
                    break;
            }
        }

        /// <summary>
        /// Navigate up in the zone grid or card list
        /// </summary>
        public void NavigateUp()
        {
            if (inHandNavigation)
            {
                // Exit hand navigation, go to my monster zone
                ExitHandNavigation();
                currentZoneRow = 1;
                inZoneNavigation = true;
            }
            else if (inZoneNavigation)
            {
                currentZoneRow = Mathf.Max(0, currentZoneRow - 1);
                
                // Handle extra monster zone spacing
                if (currentZoneRow == 2)
                {
                    // Snap to extra monster zone column
                    currentZoneCol = currentZoneCol < 2 ? 1 : 3;
                }
            }
            else
            {
                // Start zone navigation from bottom
                inZoneNavigation = true;
                currentZoneRow = 0;
            }

            OnZoneChanged?.Invoke(currentZoneRow, currentZoneCol);
            UpdateZoneHighlight();
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Navigate down in the zone grid or card list
        /// </summary>
        public void NavigateDown()
        {
            if (inHandNavigation)
            {
                // Stay in hand
                return;
            }
            else if (inZoneNavigation)
            {
                if (currentZoneRow == 0)
                {
                    // Enter hand navigation
                    EnterHandNavigation();
                }
                else
                {
                    currentZoneRow = Mathf.Min(ZONE_ROWS - 1, currentZoneRow + 1);
                    
                    // Handle extra monster zone spacing
                    if (currentZoneRow == 2)
                    {
                        currentZoneCol = currentZoneCol < 2 ? 1 : 3;
                    }
                }
            }
            else
            {
                // Start hand navigation
                EnterHandNavigation();
            }

            OnZoneChanged?.Invoke(currentZoneRow, currentZoneCol);
            UpdateZoneHighlight();
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Navigate left in the zone grid or hand
        /// </summary>
        public void NavigateLeft()
        {
            if (inHandNavigation)
            {
                currentHandIndex = Mathf.Max(0, currentHandIndex - 1);
                OnHandIndexChanged?.Invoke(currentHandIndex);
                UpdateHandHighlight();
            }
            else if (inZoneNavigation)
            {
                if (currentZoneRow == 2)
                {
                    // Extra monster zone - only 2 positions
                    currentZoneCol = currentZoneCol == 3 ? 1 : 1;
                }
                else
                {
                    currentZoneCol = Mathf.Max(0, currentZoneCol - 1);
                }
                OnZoneChanged?.Invoke(currentZoneRow, currentZoneCol);
                UpdateZoneHighlight();
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Navigate right in the zone grid or hand
        /// </summary>
        public void NavigateRight()
        {
            if (inHandNavigation)
            {
                var handCount = GetHandCardCount();
                currentHandIndex = Mathf.Min(handCount - 1, currentHandIndex + 1);
                OnHandIndexChanged?.Invoke(currentHandIndex);
                UpdateHandHighlight();
            }
            else if (inZoneNavigation)
            {
                if (currentZoneRow == 2)
                {
                    // Extra monster zone - only 2 positions
                    currentZoneCol = currentZoneCol == 1 ? 3 : 3;
                }
                else
                {
                    currentZoneCol = Mathf.Min(ZONE_COLS - 1, currentZoneCol + 1);
                }
                OnZoneChanged?.Invoke(currentZoneRow, currentZoneCol);
                UpdateZoneHighlight();
            }
            ControllerManager.Instance?.HapticNavigate();
        }

        /// <summary>
        /// Confirm selection or action
        /// </summary>
        public void ConfirmAction()
        {
            if (Time.time - lastSelectTime < cardSelectCooldown) return;
            lastSelectTime = Time.time;

            if (inHandNavigation)
            {
                SelectHandCard(currentHandIndex);
            }
            else if (inZoneNavigation)
            {
                SelectZone(currentZoneRow, currentZoneCol);
            }

            ControllerManager.Instance?.HapticConfirm();
        }

        /// <summary>
        /// Cancel current action
        /// </summary>
        public void CancelAction()
        {
            if (inHandNavigation)
            {
                ExitHandNavigation();
            }
            else if (inZoneNavigation)
            {
                inZoneNavigation = false;
                ClearHighlights();
            }
            
            ControllerManager.Instance?.HapticCancel();
        }

        /// <summary>
        /// Enter hand card navigation mode
        /// </summary>
        public void EnterHandNavigation()
        {
            inHandNavigation = true;
            inZoneNavigation = false;
            currentHandIndex = 0;
            OnHandIndexChanged?.Invoke(currentHandIndex);
            UpdateHandHighlight();
        }

        /// <summary>
        /// Exit hand navigation mode
        /// </summary>
        public void ExitHandNavigation()
        {
            inHandNavigation = false;
            ClearHandHighlight();
        }

        /// <summary>
        /// Toggle between hand navigation and zone navigation
        /// </summary>
        public void ToggleHandNavigation()
        {
            if (inHandNavigation)
            {
                ExitHandNavigation();
                inZoneNavigation = true;
                UpdateZoneHighlight();
            }
            else
            {
                EnterHandNavigation();
            }
        }

        /// <summary>
        /// Select a hand card by index
        /// </summary>
        public void SelectHandCard(int index)
        {
            var handCards = GetHandCards();
            if (index >= 0 && index < handCards.Count)
            {
                selectedCard = handCards[index];
                OnCardSelected?.Invoke(selectedCard);
                
                // Trigger card interaction
                TriggerCardInteraction(selectedCard);
            }
        }

        /// <summary>
        /// Select a zone on the field
        /// </summary>
        public void SelectZone(int row, int col)
        {
            var card = GetCardAtZone(row, col);
            if (card != null)
            {
                selectedCard = card;
                OnCardSelected?.Invoke(card);
                TriggerCardInteraction(card);
            }
            else
            {
                // Empty zone - may trigger place selector if applicable
                TriggerZoneSelection(row, col);
            }
        }

        private void TriggerCardInteraction(GameCard card)
        {
            if (card == null) return;
            
            // Show card actions/effects menu
            // This interfaces with OcgCore's existing card interaction system
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Card interaction is handled by the existing place selector system
                // We just need to simulate the selection
            }
        }

        private void TriggerZoneSelection(int row, int col)
        {
            // Convert grid position to game zone
            var location = GetLocationFromGrid(row, col);
            var sequence = GetSequenceFromCol(col);
            var controller = row <= 2 ? 0u : 1u; // My side vs opponent side
            
            // Interface with place selector system
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Zone selection for card placement
            }
        }

        private uint GetLocationFromGrid(int row, int col)
        {
            return row switch
            {
                0 => (uint)CardLocation.SpellZone,      // My spell/trap
                1 => (uint)CardLocation.MonsterZone,   // My monsters
                2 => (uint)CardLocation.MonsterZone,   // Extra monster zones
                3 => (uint)CardLocation.MonsterZone,   // Opponent monsters
                4 => (uint)CardLocation.SpellZone,     // Opponent spell/trap
                _ => 0
            };
        }

        private uint GetSequenceFromCol(int col)
        {
            return (uint)col;
        }

        /// <summary>
        /// View graveyard
        /// </summary>
        public void ViewGraveyard()
        {
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Trigger graveyard view
            }
        }

        /// <summary>
        /// View banished pile
        /// </summary>
        public void ViewBanished()
        {
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Trigger banished view
            }
        }

        /// <summary>
        /// View extra deck
        /// </summary>
        public void ViewExtraDeck()
        {
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Trigger extra deck view
            }
        }

        /// <summary>
        /// Show info for currently selected card
        /// </summary>
        public void ShowCardInfo()
        {
            if (selectedCard != null)
            {
                var ocgCore = Program.instance?.ocgcore;
                if (ocgCore != null)
                {
                    // Show card description panel
                }
            }
        }

        /// <summary>
        /// Try to end current phase
        /// </summary>
        public void TryEndPhase()
        {
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore != null && ocgCore.showing)
            {
                // Trigger phase end if available
            }
        }

        /// <summary>
        /// Set cards that can be selected (for card selection prompts)
        /// </summary>
        public void SetSelectableCards(List<GameCard> cards)
        {
            selectableCards = cards ?? new List<GameCard>();
        }

        /// <summary>
        /// Clear selectable cards
        /// </summary>
        public void ClearSelectableCards()
        {
            selectableCards.Clear();
        }

        private void UpdateZoneHighlight()
        {
            // Update visual highlight for current zone
            // This interfaces with the existing DuelBGManager
        }

        private void UpdateHandHighlight()
        {
            // Update visual highlight for current hand card
        }

        private void ClearHighlights()
        {
            ClearZoneHighlight();
            ClearHandHighlight();
        }

        private void ClearZoneHighlight()
        {
            // Clear zone highlight visuals
        }

        private void ClearHandHighlight()
        {
            // Clear hand highlight visuals
        }

        private int GetHandCardCount()
        {
            var ocgCore = Program.instance?.ocgcore;
            return ocgCore?.GetMyHandCount() ?? 0;
        }

        private List<GameCard> GetHandCards()
        {
            var result = new List<GameCard>();
            var ocgCore = Program.instance?.ocgcore;
            
            if (ocgCore != null)
            {
                // Create a snapshot copy to avoid collection modification during enumeration
                var cardsCopy = new List<GameCard>(OcgCore.cards);
                foreach (var card in cardsCopy)
                {
                    if (card != null && card.p.controller == 0 && (card.p.location & (uint)CardLocation.Hand) > 0)
                    {
                        result.Add(card);
                    }
                }
                result.Sort((a, b) => a.p.sequence.CompareTo(b.p.sequence));
            }
            
            return result;
        }

        private GameCard GetCardAtZone(int row, int col)
        {
            var ocgCore = Program.instance?.ocgcore;
            if (ocgCore == null) return null;

            uint controller = row <= 2 ? 0u : 1u;
            uint location = GetLocationFromGrid(row, col);
            uint sequence = GetSequenceFromCol(col);

            // Handle extra monster zone special case
            if (row == 2)
            {
                location = (uint)CardLocation.MonsterZone;
                sequence = col == 1 ? 5u : 6u; // Extra monster zone sequences
            }

            // Create a snapshot copy to avoid collection modification during enumeration
            var cardsCopy = new List<GameCard>(OcgCore.cards);
            foreach (var card in cardsCopy)
            {
                if (card != null &&
                    card.p.controller == controller &&
                    card.p.location == location &&
                    card.p.sequence == sequence)
                {
                    return card;
                }
            }

            return null;
        }

        /// <summary>
        /// Reset navigation state
        /// </summary>
        public void ResetNavigation()
        {
            currentZoneRow = 0;
            currentZoneCol = 2; // Center
            currentHandIndex = 0;
            inHandNavigation = false;
            inZoneNavigation = false;
            selectedCard = null;
            ClearHighlights();
        }

        /// <summary>
        /// Focus on a specific card (external call)
        /// </summary>
        public void FocusCard(GameCard card)
        {
            if (card == null) return;

            selectedCard = card;
            
            if ((card.p.location & (uint)CardLocation.Hand) > 0)
            {
                // Card is in hand
                inHandNavigation = true;
                inZoneNavigation = false;
                currentHandIndex = (int)card.p.sequence;
                UpdateHandHighlight();
            }
            else
            {
                // Card is on field
                inHandNavigation = false;
                inZoneNavigation = true;
                SetZoneFromCard(card);
                UpdateZoneHighlight();
            }

            OnCardSelected?.Invoke(card);
        }

        private void SetZoneFromCard(GameCard card)
        {
            int controller = (int)card.p.controller;
            
            if ((card.p.location & (uint)CardLocation.MonsterZone) > 0)
            {
                if (card.p.sequence >= 5) // Extra monster zone
                {
                    currentZoneRow = 2;
                    currentZoneCol = card.p.sequence == 5 ? 1 : 3;
                }
                else
                {
                    currentZoneRow = controller == 0 ? 1 : 3;
                    currentZoneCol = (int)card.p.sequence;
                }
            }
            else if ((card.p.location & (uint)CardLocation.SpellZone) > 0)
            {
                currentZoneRow = controller == 0 ? 0 : 4;
                currentZoneCol = (int)card.p.sequence;
            }
        }
    }
}
