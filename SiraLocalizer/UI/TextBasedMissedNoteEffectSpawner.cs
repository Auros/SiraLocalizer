using Polyglot;
using UnityEngine;
using Zenject;

namespace SiraLocalizer.UI
{
	public class TextBasedMissedNoteEffectSpawner : MonoBehaviour
	{
		private BeatmapObjectManager _beatmapObjectManager;
		private AudioTimeSyncController _audioTimeSyncController;
		private CoreGameHUDController.InitData _initData;

		private FlyingTextSpawner _flyingTextSpawner;
		private float _spawnPosZ;

		[Inject]
		public void Construct(DiContainer container, BeatmapObjectManager beatmapObjectManager, AudioTimeSyncController audioTimeSyncController, CoreGameHUDController.InitData initData, MissedNoteEffectSpawner original)
        {
			_beatmapObjectManager = beatmapObjectManager;
			_audioTimeSyncController = audioTimeSyncController;
			_initData = initData;

			_flyingTextSpawner = container.InstantiateComponent<ItalicizedFlyingTextSpawner>(gameObject);

			Destroy(original.gameObject);
		}

		public void Start()
		{
			if (_initData.hide)
			{
				enabled = false;
				return;
			}

			_beatmapObjectManager.noteWasMissedEvent += HandleNoteWasMissed;
			_spawnPosZ = transform.position.z;
		}

		public void OnDestroy()
		{
			if (_beatmapObjectManager != null)
			{
				_beatmapObjectManager.noteWasMissedEvent -= HandleNoteWasMissed;
			}
		}

		public void HandleNoteWasMissed(NoteController noteController)
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
	}
}
