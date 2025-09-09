using BGLib.Polyglot;
using UnityEngine;

namespace SiraLocalizer.UI
{
    internal class TextBasedMissedNoteEffectSpawner : MissedNoteEffectSpawner
    {
        [SerializeField]
        private FlyingTextSpawner _flyingTextSpawner;

        internal void Init(FlyingTextSpawner flyingTextSpawner)
        {
            _flyingTextSpawner = flyingTextSpawner;
        }

        public new void HandleNoteWasMissed(NoteController noteController)
        {
            if (noteController.hidden) return;
            if (noteController.noteData.time + 0.5f < _audioTimeSyncController.songTime) return;
            if (noteController.noteData.colorType == ColorType.None) return;

            Vector3 position = noteController.noteTransform.position;
            Quaternion worldRotation = noteController.worldRotation;

            position = noteController.inverseWorldRotation * position;
            position.x = Mathf.Sign(position.x);
            position.z = _spawnPosZ;
            position = worldRotation * position;

            _flyingTextSpawner.SpawnText(position, noteController.worldRotation, noteController.inverseWorldRotation, Localization.Get("FLYING_TEXT_MISS"));
        }
    }
}
