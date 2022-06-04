using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarienEngine
{
    public delegate void CommandPointClicked();
    public class CommandSticker : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        public Sprite[] spriteArray;
        public float frameRate = 0.1f;
        private int currentFrame = 0;
        private float frameTimer;
        private int frameCount;
        private CommandPointClicked commandPointClickedCallback;

        // Start is called before the first frame update
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            frameCount = spriteArray.Length;
            frameTimer = frameRate;
        }

        // Update is called once per frame
        void Update()
        {
            if (spriteArray.Length > 1)
            {
                frameTimer -= Time.deltaTime;
                if (frameTimer <= 0f)
                {
                    frameTimer += frameRate;
                    currentFrame = (currentFrame + 1) % frameCount;
                    spriteRenderer.sprite = spriteArray[currentFrame];
                }
            }
        }

        void OnMouseUp()
        {
            // If this sticker is clicked, propagate event up so it can be removed from list
            if (commandPointClickedCallback != null)
                commandPointClickedCallback();
        }

        public void OnClick(CommandPointClicked callback)
        {
            commandPointClickedCallback = callback;
        }
    }
}