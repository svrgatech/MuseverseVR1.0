using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Sprite))]
public class WaveformPreviewCreator : MonoBehaviour
{
    public int width = 1024;
    public int height = 64;
    public Color background = Color.black;
    public Color foreground = Color.yellow;
    public GameObject arrow = null;
    
    public Image waveformImg = null;
    public Image progressIndicatorWaveformImg;
    private int samplesize;
    private float[] samples = null;
    private float[] waveform = null;
    private float arrowoffsetx;

    private void Update()
    {
        //// move the arrow
        //float xoffset = (aud.time / aud.clip.length) * sprend.size.x;
        //arrow.transform.position = new Vector3(xoffset + arrowoffsetx, 0);
    }
    //private Texture2D GetWaveform(AudioClip clip)
    //{
    //    int halfheight = height / 2;
    //    float heightscale = (float)height * 0.75f;

    //    // get the sound data
    //    Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
    //    waveform = new float[width];

    //    samplesize = clip.samples * clip.channels;
    //    samples = new float[samplesize];
    //    clip.GetData(samples, 0);

    //    int packsize = (samplesize / width);
    //    for (int w = 0; w < width; w++)
    //    {
    //        waveform[w] = Mathf.Abs(samples[w * packsize]);
    //    }

    //    // map the sound data to texture
    //    // 1 - clear
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            tex.SetPixel(x, y, background);
    //        }
    //    }

    //    // 2 - plot
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < waveform[x] * heightscale; y++)
    //        {
    //            tex.SetPixel(x, halfheight + y, foreground);
    //            tex.SetPixel(x, halfheight - y, foreground);
    //        }
    //    }

    //    tex.Apply();

    //    return tex;
    //}

    private Texture2D GetWaveform(AudioClip clip)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[width];
        samplesize = clip.samples * clip.channels;
        samples = new float[samplesize];
        clip.GetData(samples, 0);

        int packsize = samplesize / width;
        float maxWaveformValue = 0f;
        for (int i = 0; i < width; i++)
        {
            float wavePeak = 0f;
            for (int j = 0; j < packsize; j++)
            {
                int sampleIndex = i * packsize + j;
                if (sampleIndex < samplesize)
                {
                    wavePeak = Mathf.Max(wavePeak, Mathf.Abs(samples[sampleIndex]));
                }
            }
            maxWaveformValue = Mathf.Max(maxWaveformValue, wavePeak);
            waveform[i] = wavePeak;
        }

        // Normalize the waveform
        for (int i = 0; i < width; i++)
        {
            waveform[i] /= maxWaveformValue;
        }

        // Draw the full waveform on the texture
        tex.SetPixels(Enumerable.Repeat(background, width * height).ToArray());
        for (int x = 0; x < width; x++)
        {
            int waveHeight = (int)(waveform[x] * height);
            for (int y = (height - waveHeight) / 2; y < (height + waveHeight) / 2; y++)
            {
                tex.SetPixel(x, y, foreground);
            }
        }
        tex.Apply();

        // Now create the segmented texture with rounded tapering
        Texture2D segmentedTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        segmentedTex.SetPixels(Enumerable.Repeat(new Color(0, 0, 0, 0), width * height).ToArray());

        int barWidth = 15; // Width of each bar
        int barSpacing = 5; // Space between bars
        int taperHeight = 10; // Tapering effect's height

        for (int i = 0; i < width; i += (barWidth + barSpacing))
        {
            int taperWidth = barWidth / 2;
            for (int x = i; x < i + barWidth && x < width; x++)
            {
                int waveHeight = (int)(waveform[x] * (height - 2 * taperHeight));
                int centerY = height / 2;
                int halfWaveHeight = waveHeight / 2;

                // Calculate taper offset with a rounded effect
                int distanceFromCenter = Mathf.Abs(x - (i + taperWidth));
                float taperFactor = (float)(taperWidth - distanceFromCenter) / taperWidth;
                taperFactor = taperFactor * taperFactor; // Apply square to create a rounded effect
                int taperOffset = Mathf.FloorToInt(taperHeight * taperFactor);

                // Draw the waveform with rounded tapering
                for (int y = centerY - halfWaveHeight + taperOffset; y < centerY + halfWaveHeight - taperOffset; y++)
                {
                    if (y >= 0 && y < height) // Check within texture bounds
                    {
                        segmentedTex.SetPixel(x, y, foreground);
                    }
                }
            }
        }

        segmentedTex.Apply();
        return segmentedTex;
    }










    public void CreateWaveformPreview(AudioClip clip)
    {
        // reference components on the gameobject
        Texture2D texwav = GetWaveform(clip);
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        waveformImg.sprite = Sprite.Create(texwav, rect, Vector2.zero);
        waveformImg.color = new Color(1, 1, 1, 1);
        

        //Create the preview progress waveform
        CreateProgressWaveformImg(waveformImg.sprite);

        //arrow.transform.position = new Vector3(0f, 0f);
        //arrowoffsetx = -(arrow.GetComponent<SpriteRenderer>().size.x / 2f);

       //cam.transform.position = new Vector3(0f, 0f, -1f);
       //cam.transform.Translate(Vector3.right * (sprend.size.x / 2f));

    }


    //This is to create a progress indicator, which is basically the waveform of the original
    //Audio clip, duplicated and coloured as a filled sprite. Instead of a progress bar, we use this,
    //For a better visual effect
    public void CreateProgressWaveformImg(Sprite sprite)
    {
        if (progressIndicatorWaveformImg)
        {
            progressIndicatorWaveformImg.sprite = sprite;
            progressIndicatorWaveformImg.color = Color.blue;

            progressIndicatorWaveformImg.fillMethod = Image.FillMethod.Horizontal;
            progressIndicatorWaveformImg.fillAmount = 0;
        }
    }

    public void UpdatePlaybackProgressFill(float value)
    {
        if(progressIndicatorWaveformImg && progressIndicatorWaveformImg.sprite!=null)
            progressIndicatorWaveformImg.fillAmount = value;
    }


}
