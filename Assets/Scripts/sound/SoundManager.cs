using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultipleBattle
{
	public class SoundManager : SingleMonoBehaviour<SoundManager>
	{

		public AudioSource bgmAudioSource;
		public AudioSource seAudioSource;

		public AudioClip bgm;
		public AudioClip se;
		public AudioClip hit;

		public void PlayBGM ()
		{
			bgmAudioSource.clip = bgm;
			bgmAudioSource.loop = true;
			bgmAudioSource.Play ();
		}

		public void PlaySE (AudioClip clip)
		{
			seAudioSource.PlayOneShot (clip);
		}

	}
}