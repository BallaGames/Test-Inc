using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class SteamHelper
{
    public static Texture2D GetSteamImageAsTexture2D(int iImage)
    {
        Texture2D ret = null;
        uint imageWidth, imageHeight;
        bool isValid = SteamUtils.GetImageSize(iImage, out imageWidth, out imageHeight);
        if (isValid)
        {
            //Image width x image height x 4 bytes (one byte each for r, g, b, a)
            byte[] image = new byte[imageWidth * imageHeight * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(imageWidth * imageHeight * 4));
            if (isValid)
            {
                ret = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, true);
                ret.LoadRawTextureData(image);
                ret.Apply();
            }
        }
        return ret;
    }
}
