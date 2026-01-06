using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MDPro3.ControllerSupport
{
    /// <summary>
    /// Provides enhanced menu navigation support for controller input,
    /// including visual feedback, automatic selection, and improved focus management.
    /// </summary>
    public class ControllerMenuNavigation : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.3f, 1f);
        [SerializeField] private float highlightScale = 1.05f;
        [SerializeField] private float transitionDuration = 0.15f;

        [Header("Navigation")]
        [SerializeField] private bool wrapNavigation = true;
        [SerializeField] private float navigationCooldown = 0.2f;

        private Selectable currentSelection;
        private List<Selectable> selectables = new List<Selectable>();
        private float lastNavigationTime;
        private RectTransform selectionIndicator;
        private bool isControllerActive;

        private static ControllerMenuNavigation instance;
        public static ControllerMenuNavigation Instance => instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
        }

        private void OnEnable()
        {
            ControllerManager.OnControllerConnected += OnControllerConnected;
            ControllerManager.OnControllerDisconnected += OnControllerDisconnected;
            ControllerManager.OnGameActionTriggered += OnGameAction;

            isControllerActive = ControllerManager.IsControllerConnected;
        }

        private void OnDisable()
        {
            ControllerManager.OnControllerConnected -= OnControllerConnected;
            ControllerManager.OnControllerDisconnected -= OnControllerDisconnected;
            ControllerManager.OnGameActionTriggered -= OnGameAction;
        }

        private void Update()
        {
            if (!isControllerActive) return;

            // Track current selection
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current != null)
            {
                var selectable = current.GetComponent<Selectable>();
                if (selectable != currentSelection)
                {
                    OnSelectionChanged(selectable);
                }
            }
        }

        private void OnControllerConnected(ControllerType type)
        {
            isControllerActive = true;
            
            // Auto-select first element if nothing is selected
            if (EventSystem.current?.currentSelectedGameObject == null)
            {
                SelectFirstAvailable();
            }
        }

        private void OnControllerDisconnected()
        {
            isControllerActive = false;
            HideSelectionIndicator();
        }

        private void OnGameAction(GameAction action)
        {
            if (Time.unscaledTime - lastNavigationTime < navigationCooldown)
                return;

            switch (action)
            {
                case GameAction.NavigateUp:
                    NavigateDirection(MoveDirection.Up);
                    break;
                case GameAction.NavigateDown:
                    NavigateDirection(MoveDirection.Down);
                    break;
                case GameAction.NavigateLeft:
                    NavigateDirection(MoveDirection.Left);
                    break;
                case GameAction.NavigateRight:
                    NavigateDirection(MoveDirection.Right);
                    break;
                case GameAction.Confirm:
                    ConfirmSelection();
                    break;
                case GameAction.Cancel:
                    CancelSelection();
                    break;
            }
        }

        /// <summary>
        /// Navigate in the specified direction
        /// </summary>
        public void NavigateDirection(MoveDirection direction)
        {
            var current = EventSystem.current?.currentSelectedGameObject?.GetComponent<Selectable>();
            if (current == null)
            {
                SelectFirstAvailable();
                return;
            }

            Selectable next = null;
            switch (direction)
            {
                case MoveDirection.Up:
                    next = current.FindSelectableOnUp();
                    break;
                case MoveDirection.Down:
                    next = current.FindSelectableOnDown();
                    break;
                case MoveDirection.Left:
                    next = current.FindSelectableOnLeft();
                    break;
                case MoveDirection.Right:
                    next = current.FindSelectableOnRight();
                    break;
            }

            if (next != null && next.IsInteractable())
            {
                SelectElement(next);
                lastNavigationTime = Time.unscaledTime;
                ControllerManager.Instance?.HapticNavigate();
            }
            else if (wrapNavigation)
            {
                // Wrap navigation to opposite side
                WrapNavigation(direction);
            }
        }

        /// <summary>
        /// Wrap navigation to the opposite side of the UI
        /// </summary>
        private void WrapNavigation(MoveDirection direction)
        {
            RefreshSelectablesList();
            if (selectables.Count == 0) return;

            var current = EventSystem.current?.currentSelectedGameObject?.GetComponent<Selectable>();
            if (current == null) return;

            Selectable target = null;
            var currentPos = current.transform.position;

            switch (direction)
            {
                case MoveDirection.Up:
                    // Find bottom-most element in the same column
                    target = FindBottomMostInColumn(currentPos.x);
                    break;
                case MoveDirection.Down:
                    // Find top-most element in the same column
                    target = FindTopMostInColumn(currentPos.x);
                    break;
                case MoveDirection.Left:
                    // Find right-most element in the same row
                    target = FindRightMostInRow(currentPos.y);
                    break;
                case MoveDirection.Right:
                    // Find left-most element in the same row
                    target = FindLeftMostInRow(currentPos.y);
                    break;
            }

            if (target != null && target.IsInteractable())
            {
                SelectElement(target);
                lastNavigationTime = Time.unscaledTime;
            }
        }

        private Selectable FindTopMostInColumn(float x)
        {
            Selectable top = null;
            float topY = float.MinValue;
            const float tolerance = 50f;

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                var pos = s.transform.position;
                if (Mathf.Abs(pos.x - x) < tolerance && pos.y > topY)
                {
                    topY = pos.y;
                    top = s;
                }
            }
            return top;
        }

        private Selectable FindBottomMostInColumn(float x)
        {
            Selectable bottom = null;
            float bottomY = float.MaxValue;
            const float tolerance = 50f;

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                var pos = s.transform.position;
                if (Mathf.Abs(pos.x - x) < tolerance && pos.y < bottomY)
                {
                    bottomY = pos.y;
                    bottom = s;
                }
            }
            return bottom;
        }

        private Selectable FindLeftMostInRow(float y)
        {
            Selectable left = null;
            float leftX = float.MaxValue;
            const float tolerance = 50f;

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                var pos = s.transform.position;
                if (Mathf.Abs(pos.y - y) < tolerance && pos.x < leftX)
                {
                    leftX = pos.x;
                    left = s;
                }
            }
            return left;
        }

        private Selectable FindRightMostInRow(float y)
        {
            Selectable right = null;
            float rightX = float.MinValue;
            const float tolerance = 50f;

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                var pos = s.transform.position;
                if (Mathf.Abs(pos.y - y) < tolerance && pos.x > rightX)
                {
                    rightX = pos.x;
                    right = s;
                }
            }
            return right;
        }

        /// <summary>
        /// Confirm the current selection (equivalent to pressing A/Cross)
        /// </summary>
        public void ConfirmSelection()
        {
            var current = EventSystem.current?.currentSelectedGameObject;
            if (current == null) return;

            var button = current.GetComponent<Button>();
            if (button != null && button.IsInteractable())
            {
                button.onClick.Invoke();
                ControllerManager.Instance?.HapticConfirm();
                return;
            }

            var toggle = current.GetComponent<Toggle>();
            if (toggle != null && toggle.IsInteractable())
            {
                toggle.isOn = !toggle.isOn;
                ControllerManager.Instance?.HapticConfirm();
                return;
            }

            // Handle other interactable types as needed
        }

        /// <summary>
        /// Cancel/go back (equivalent to pressing B/Circle)
        /// </summary>
        public void CancelSelection()
        {
            ControllerManager.Instance?.HapticCancel();
            // Cancel action is typically handled by the current servant/screen
        }

        /// <summary>
        /// Select a specific UI element
        /// </summary>
        public void SelectElement(Selectable selectable)
        {
            if (selectable == null || !selectable.IsInteractable()) return;
            
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            OnSelectionChanged(selectable);
        }

        /// <summary>
        /// Select the first available interactable element
        /// </summary>
        public void SelectFirstAvailable()
        {
            RefreshSelectablesList();
            
            foreach (var s in selectables)
            {
                if (s.IsInteractable())
                {
                    SelectElement(s);
                    return;
                }
            }
        }

        /// <summary>
        /// Refresh the list of selectable elements in the scene
        /// </summary>
        public void RefreshSelectablesList()
        {
            selectables.Clear();
            selectables.AddRange(FindObjectsOfType<Selectable>());
            
            // Sort by position (top-left to bottom-right)
            selectables.Sort((a, b) =>
            {
                var posA = a.transform.position;
                var posB = b.transform.position;
                var yDiff = posB.y.CompareTo(posA.y);
                return yDiff != 0 ? yDiff : posA.x.CompareTo(posB.x);
            });
        }

        /// <summary>
        /// Called when selection changes
        /// </summary>
        private void OnSelectionChanged(Selectable newSelection)
        {
            if (newSelection == currentSelection) return;

            // Reset previous selection visual
            if (currentSelection != null)
            {
                ResetVisual(currentSelection);
            }

            currentSelection = newSelection;

            // Apply new selection visual
            if (currentSelection != null && isControllerActive)
            {
                ApplySelectionVisual(currentSelection);
                UpdateSelectionIndicator(currentSelection);
            }
        }

        private void ApplySelectionVisual(Selectable selectable)
        {
            // Add subtle scale effect
            var rect = selectable.GetComponent<RectTransform>();
            if (rect != null)
            {
                // Store original scale if not already stored
                if (!originalScales.ContainsKey(selectable))
                {
                    originalScales[selectable] = rect.localScale;
                }

                rect.localScale = originalScales[selectable] * highlightScale;
            }
        }

        private void ResetVisual(Selectable selectable)
        {
            var rect = selectable.GetComponent<RectTransform>();
            if (rect != null && originalScales.ContainsKey(selectable))
            {
                rect.localScale = originalScales[selectable];
            }
        }

        private Dictionary<Selectable, Vector3> originalScales = new Dictionary<Selectable, Vector3>();

        private void UpdateSelectionIndicator(Selectable selectable)
        {
            // Selection indicator position update logic
            // This can be used for custom selection graphics
        }

        private void HideSelectionIndicator()
        {
            if (selectionIndicator != null)
                selectionIndicator.gameObject.SetActive(false);
        }

        /// <summary>
        /// Find the nearest selectable to a given world position
        /// </summary>
        public Selectable FindNearestSelectable(Vector3 worldPosition)
        {
            RefreshSelectablesList();
            
            Selectable nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                
                var dist = Vector3.Distance(s.transform.position, worldPosition);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = s;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Get all selectables in a specific region
        /// </summary>
        public List<Selectable> GetSelectablesInRegion(Rect region)
        {
            RefreshSelectablesList();
            var result = new List<Selectable>();

            foreach (var s in selectables)
            {
                if (!s.IsInteractable()) continue;
                
                var pos = s.transform.position;
                if (region.Contains(new Vector2(pos.x, pos.y)))
                {
                    result.Add(s);
                }
            }

            return result;
        }
    }
}
