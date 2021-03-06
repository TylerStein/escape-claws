using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;


[System.Serializable]
public class DialogSequence
{
    [SerializeField] public List<string> lines;
    [SerializeField] public Transform speakerTransform;
    [SerializeField] public AudioSource speakSound;
}


public class DialogManager : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI textGui;
    public Transform canvasTransform;

    public bool IsInDialog { get => _currentSequence != null && _currentSequence.Count > 0; }

    [SerializeField] private List<DialogSequence> _currentSequence;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private PauseManager _pauseManager;
    [SerializeField] private CameraController _cameraController;

    [SerializeField] private int _sequenceIndex = 0;
    [SerializeField] private int _lineIndex = 0;

    [SerializeField] private Transform _cameraAnchor;
    private UnityEvent _currentEndEvent;

    private void Start() {
        if (!_cameraController) _cameraController = FindObjectOfType<CameraController>();
        if (!_pauseManager) _pauseManager = FindObjectOfType<PauseManager>();
        if (!_playerInput) _playerInput = FindObjectOfType<PlayerInput>();
        if (!IsInDialog) EndDialog();
    }

    private void Update() {
        if (_playerInput.UseDown) {
            if (IsInDialog && _pauseManager.IsInPauseMenu == false) {
                NextLine();
            }
        }
    }

    public void StartDialog(List<DialogSequence> sequence, UnityEvent finishEvent = null) {
        _lineIndex = 0;
        _sequenceIndex = 0;
        if (sequence == null || sequence.Count == 0) {
            _currentEndEvent = null;
            _currentSequence = null;
            Debug.LogWarning("StartDialog called with empty sequence");
        } else {
            _currentEndEvent = finishEvent;
            _currentSequence = sequence;
            _pauseManager.OnPauseDialogOpen();
            DisplayCurrentDialog();
        }
    }

    void EndDialog() {
        if (_currentEndEvent != null) _currentEndEvent.Invoke();
        _currentSequence = null;
        _currentEndEvent = null;
        canvasGroup.alpha = 0;
        _pauseManager.OnPauseDialogClose();
        _cameraController.targetTransform = _cameraAnchor;
    }

    void DisplayCurrentDialog() {
        // show new dialog
        DialogSequence activeSequence = _currentSequence[_sequenceIndex];
        if (activeSequence != null) {
            canvasTransform.position = activeSequence.speakerTransform.position;
            canvasGroup.alpha = 1;
            textGui.text = activeSequence.lines[_lineIndex];
            activeSequence.speakSound.Play();
            _cameraController.targetTransform = activeSequence.speakerTransform;
        }
    }

    void NextLine() {
        if ((_lineIndex + 1) >= _currentSequence[_sequenceIndex].lines.Count) {
            // next line
            NextMainSequence();
        } else {
            _lineIndex++;
            DisplayCurrentDialog();
        }
    }

    void NextMainSequence() {
        if ((_sequenceIndex + 1) >= _currentSequence.Count) {
            // end sequence
            EndDialog();
        } else {
            _lineIndex = 0;
            _sequenceIndex++;
            DisplayCurrentDialog();
        }
    }
}
