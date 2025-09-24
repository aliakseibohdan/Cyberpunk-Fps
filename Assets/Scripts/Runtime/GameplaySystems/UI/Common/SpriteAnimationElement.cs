using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Elements
{
    public enum CycleType
    {
        Loop,
        Reverse,
        PingPong,
        OneShot,
    }

    [UxmlElement]
    public partial class SpriteAnimationElement : Image
    {
        [UxmlAttribute("start-delay")]
        private float startDelay = 0f;

        [UxmlAttribute("fps")]
        private int fps = 30;

        [UxmlAttribute("cycle")]
        private CycleType cycle = CycleType.Loop;

        [UxmlAttribute("sprites")]
        private List<Sprite> sprites = new();

        private int _spriteIdx = 0;
        private int _increment = 1;
        private long _frameTime;
        private long _startDelay;

        private IVisualElementScheduledItem _startTask;


        #region Constructors

        public SpriteAnimationElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        public SpriteAnimationElement(List<Sprite> sprites) : this()
        {
            this.sprites = sprites;
        }

        public SpriteAnimationElement(List<Sprite> sprites, float startDelay) : this(sprites)
        {
            this.startDelay = startDelay;
        }

        public SpriteAnimationElement(List<Sprite> sprites, float startDelay, int fps) : this(sprites, startDelay)
        {
            this.fps = fps;
        }

        public SpriteAnimationElement(List<Sprite> sprites, float startDelay, int fps, CycleType cycle) : this(sprites, startDelay, fps)
        {
            this.cycle = cycle;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Play(_startDelay, true);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            Pause();
        }

        private void HandleFPSAndDelay()
        {
            _frameTime = (long)((1f / fps) * 1000);
            _startDelay = (long)(startDelay * 1000);
        }

        #endregion


        #region API

        /// <summary>
        /// Update the sprites.
        /// </summary>
        /// <param name="newSprites"></param>
        /// <param name="autoPlay"></param>
        public void UpdateSprites(List<Sprite> newSprites, bool autoPlay = false)
        {
            Pause();
            this.sprites = newSprites;

            if (autoPlay)
                Play();
        }

        /// <summary>
        /// Play the animation.
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="restart"></param>
        public void Play(long delay = 0, bool restart = false)
        {
            Pause();
            HandleFPSAndDelay();
            if (restart)
            {
                _spriteIdx = 0;
                _increment = 1;
            }

            if (sprites.Count == 0)
                return;

            _startTask = schedule.Execute(() =>
            {
                this.sprite = sprites[_spriteIdx];
                _spriteIdx += _increment;

                switch (cycle)
                {
                    case CycleType.Loop when _spriteIdx >= sprites.Count:
                        _spriteIdx = 0;
                        _increment = 1;
                        break;
                    case CycleType.Reverse when _spriteIdx == sprites.Count - 1:
                        _increment = -1;
                        break;
                    case CycleType.PingPong when _spriteIdx == sprites.Count - 1:
                        _increment = -1;
                        break;
                    case CycleType.PingPong when _spriteIdx == 0:
                        _increment = 1;
                        break;
                }
            })
                                 .StartingIn(delay)
                                 .Every(_frameTime)
                                 .Until(() =>
                                 {
                                     var result = false;
                                     switch (cycle)
                                     {
                                         case CycleType.OneShot when _spriteIdx == sprites.Count - 1:
                                         case CycleType.Reverse when _spriteIdx == 0 && _increment == -1:
                                             result = true;
                                             break;
                                     }

                                     return result;
                                 });
        }

        /// <summary>
        /// Pause the animation.
        /// </summary>
        public void Pause() => _startTask?.Pause();

        /// <summary>
        /// Stop the animation and set the sprite to the first frame.
        /// </summary>
        public void Stop()
        {
            Pause();
            _spriteIdx = 0;
            this.sprite = sprites[_spriteIdx];
        }

        #endregion
    }
}