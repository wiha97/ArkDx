using System.Media;

namespace ArkDx.Data
{
    public class Sound
    {
        private SoundPlayer sound;
        public void SoftError()
        {
            sound = new SoundPlayer(Settings.SoftErrorSFX);
            sound.Play();
        }

        public void ErrorSound()
        {
            sound = new SoundPlayer(Settings.ErrorSFX);
            sound.Play();
        }

        public void NewFilm()
        {
            sound = new SoundPlayer(Settings.NewFilmSFX);
            sound.Play();
        }

        public void FilmSound()
        {
            sound = new SoundPlayer(Settings.SavedFilmSFX);
            sound.Play();
        }

        public void DeleteSound()
        {
            sound = new SoundPlayer(Settings.DeleteSFX);
            sound.Play();
        }

        public void CarnageSound()
        {
            sound = new SoundPlayer(Settings.CarnageSFX);
            sound.Play();
        }

        public void Custom(string blam)
        {
            sound = new SoundPlayer(blam);
            sound.Play();
        }
    }
}
