using Zenject;
using Polyglot;
using UnityEngine;

namespace SiraLocalizer.UI
{
	internal class TextBasedMissedNoteEffectSpawner : MissedNoteEffectSpawner
	{
		private DiContainer _container;
		private FlyingTextSpawner _flyingTextSpawner;

        public override void Start()
        {

        }

        [Inject]
		public void Construct(DiContainer container)
        {
			_container = container;
			var original = gameObject.GetComponent<MissedNoteEffectSpawner>();
			Destroy(original);
			if (_initData.hide)
			{
                enabled = false;
				return;
			}
			_beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
			_flyingTextSpawner = _container.InstantiateComponent<ItalicizedFlyingTextSpawner>(gameObject);
			_spawnPosZ = transform.position.z;
		}

        public override void HandleNoteWasMissed(NoteController noteController)
		{
			if (noteController.hide) return;
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

        public override void OnDestroy()
        {
			_beatmapObjectManager.noteWasMissedEvent -= HandleNoteWasMissed;
        }
    }
}