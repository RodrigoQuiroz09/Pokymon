using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimator
{
    private SpriteRenderer renderer;
    private List<Sprite> animFrames;
    public List<Sprite> AnimFrames => animFrames;
    private float framteRate;

    private int currentFrame;
    private float timer;

    public CustomAnimator(SpriteRenderer renderer, List<Sprite> animFrames, float framteRate = 0.25f)
    {
        this.renderer = renderer;
        this.animFrames = animFrames;
        this.framteRate = framteRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0f;
        renderer.sprite = animFrames[currentFrame];
    }

    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        if (timer > framteRate)
        {
            currentFrame = (currentFrame + 1) % animFrames.Count;
            renderer.sprite = animFrames[currentFrame];
            timer -= framteRate;
        }
    }
}
