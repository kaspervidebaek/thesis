﻿using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmeTests
{
    partial class Algorithm
    {
        public struct ReturnSet
        {
            public int nextCost;
            public int[] MImages;
            public int[] imageChoices;
            public ReturnSet(int nc, int[] mi, int[] ic)
            {
                nextCost = nc;
                MImages = mi;
                imageChoices = ic;
            }
        }

        static public List<Image> MosaicFirstSecondAlgorithm(Image image, List<Image> imageList)
        {
            int picturesPerRow = 3;
            int pixelsPerPicture = 1;
            double imageRatio = 1.0d;
            int newWidth = image.width;
            int newHeight = image.height;

            Image tempbmap = image;
            Image bmap = image;
            var resized = image;

            double imageRatios = 151.0 / 100.0;
            imageRatios = imageRatio;
            int height = bmap.height;
            int width = bmap.width;
            int xResolution = picturesPerRow * pixelsPerPicture;
            int mosaicWidth = width / xResolution;
            int mosaicHeight = (int)(mosaicWidth / imageRatios);
            int yResolution = (int)(height / mosaicHeight);
            int pixelsPerBlock = mosaicHeight * mosaicWidth;
            int picturesPerColumn = yResolution / pixelsPerPicture;
            Pixel[,] colorMap = new Pixel[xResolution, yResolution];

            List<Image> PicturesColor = new List<Image>();
            List<Image> PicturesResized = new List<Image>();
            Dictionary<int, int> nrOfUses = new Dictionary<int, int>();
            int counter = 0;
            int mWidth = pixelsPerPicture * mosaicWidth;
            int mHeight = pixelsPerPicture * mosaicHeight;
            bestForImage = new int[imageList.Count];
            int[] pictureMap = new int[picturesPerRow * picturesPerColumn];
            foreach (var bitMap in imageList)
            {

                bestForImage[counter] = -1;
                nrOfUses.Add(counter, 0);
                PicturesColor.Add(bitMap);
                PicturesResized.Add(bitMap);
                counter++;
            }


            int setPictureCounter = 0;
            int nrOfPics = picturesPerRow * picturesPerColumn;
            for (int i = 0; i < nrOfPics; i++)
            {
                pictureMap[i] = -1;
            }
            cost = new int[nrOfPics, nrOfPics];
            ordered = new int[nrOfPics, nrOfPics];
            currentImgList = new int[nrOfPics];
            for (int x = 0; x < picturesPerRow; x++)
            {
                for (int y = 0; y < picturesPerColumn; y++)
                {
                    int currentNr = x * picturesPerColumn + y;
                    SortedList<int, int> orderedPics = new SortedList<int, int>();
                    for (int i = 0; i < PicturesResized.Count; i++)//Color matchColor in PicturesColor)
                    {
                        int tempVal = matchValue(PicturesColor[i], resized, x * pixelsPerPicture, y * pixelsPerPicture);
                        while (orderedPics.ContainsKey(tempVal))
                            tempVal++;
                        orderedPics.Add(tempVal, i);


                    }
                    for (int i = 0; i < nrOfPics; i++)
                    {
                        cost[currentNr, i] = orderedPics.Keys[i];
                        ordered[currentNr, i] = orderedPics.Values[i];
                    }

                    ReturnSet assignments = findBestAssign(currentNr, currentImgList[currentNr], currentImgList);
                    int[] mImages = assignments.MImages;
                    int[] imageChoices = assignments.imageChoices;
                    for (int i = 0; i < assignments.imageChoices.Length; i++)
                    {
                        int mImg = mImages[i];
                        int current = imageChoices[i];
                        currentImgList[mImg] = current;
                        bestForImage[ordered[mImg, current]] = mImg;
                        if (pictureMap[mImg] == -1)
                            setPictureCounter++;
                        pictureMap[mImg] = ordered[mImg, current];

                    }
                    if (setPictureCounter == nrOfPics)
                        break;
                }

                if (setPictureCounter == nrOfPics)
                    break;
            }

            return pictureMap.ToList().Select(x => imageList[x]).ToList();

            /*
            byte tempColR = 0;
            byte tempColG = 0;
            byte tempColB = 0;
            for (int x = 0; x < picturesPerRow; x++)
            {
                for (int y = 0; y < picturesPerColumn; y++)
                {
                    int currentNr = x * picturesPerColumn + y;
                    int img = pictureMap[currentNr];
                    var matchedBit = PicturesResized[img];
                    totalDiff += cost[currentNr, currentImgList[currentNr]];
                    for (int i = 0; i < mWidth; i++)
                    {
                        for (int j = 0; j < mHeight; j++)
                        {
                            tempColR = matchedBit.Data[j, i, 0];
                            tempColG = matchedBit.Data[j, i, 1];
                            tempColB = matchedBit.Data[j, i, 2];
                            int xTemp = mosaicWidth * pixelsPerPicture * (x) + i;
                            int yTemp = mosaicHeight * pixelsPerPicture * (y) + j;
                            if (xTemp < bmap.Width && yTemp < bmap.Height)
                            {
                                bmap.Data[yTemp, xTemp, 0] = tempColR;
                                bmap.Data[yTemp, xTemp, 1] = tempColG;
                                bmap.Data[yTemp, xTemp, 2] = tempColB;
                            }
                        }
                    }
                }
            }
            totalDiff += 0;
            _currentBitmap = (Bitmap)bmap.ToBitmap().Clone();
            Crop(0, 0, picturesPerRow * mWidth, picturesPerColumn * mHeight);
            //_currentBitmap = (Bitmap)resized.Clone();*/
        }


        public static int[] bestForImage;
        public static int[,] cost;
        public static int[,] ordered;
        public static int[] currentImgList;
        public static ReturnSet findBestAssign(int mImg, int currentOrder, int[] CurrentImgList)
        {
            int current = currentOrder;
            int next = current + 1;
            int currentImage = ordered[mImg, current];
            int nextMImg = bestForImage[currentImage];
            if (nextMImg == -1)
            {
                //bestForImage[current] = mImg;
                return new ReturnSet(cost[mImg, current], new int[] { mImg }, new int[] { current });
            }
            int[] changeCIL = (int[])CurrentImgList.Clone();
            changeCIL[nextMImg]++;
            ReturnSet change = findBestAssign(nextMImg, (CurrentImgList[nextMImg] + 1), changeCIL);

            int[] stayCIL = (int[])CurrentImgList.Clone();
            stayCIL[mImg]++;
            ReturnSet stay = findBestAssign(mImg, next, stayCIL);

            int costChange = cost[nextMImg, currentImgList[nextMImg] + 1] - cost[nextMImg, currentImgList[nextMImg]]
                            + change.nextCost;
            int costStay = cost[mImg, next] - cost[mImg, current]
                            + change.nextCost;
            if (costChange > costStay)
                return new ReturnSet(costStay, combine(mImg, stay.MImages), combine(current, stay.imageChoices));
            else
                return new ReturnSet(costChange, combine(mImg, change.MImages), combine(current, change.imageChoices));

        }

        public static int[] combine(int fs, int[] snd)
        {
            int[] temp = new int[snd.Length + 1];
            for (int i = 0; i < snd.Length; i++)
            {
                temp[i] = snd[i];
            }
            temp[snd.Length] = fs;
            return temp;
        }


        public static int matchValue(Image fs, Image snd, int x, int y)
        {
            //value += fs.values[j, i].CompareTo(snd.values[y + j, x + i]);
            return fs.average.CompareTo(snd.average);
        }

        struct imageAndList
        {
            public SortedList<int, int> list;
            public int image;
            public imageAndList(int img, SortedList<int, int> li)
            {
                image = img;
                list = li;
            }
        }

        static public List<Image> MosaicGreedyByMImages(Image image, List<Image> imageList)
        {
            int picturesPerRow = 3;
            int pixelsPerPicture = 1;
            double imageRatio = 1.0d;
            int newWidth = image.width;
            int newHeight = image.height;

            Image tempbmap = image;
            Image bmap = image;

            double imageRatios = 151.0 / 100.0;
            imageRatios = imageRatio;
            int height = bmap.height;
            int width = bmap.width;
            int xResolution = picturesPerRow * pixelsPerPicture;
            int mosaicWidth = width / xResolution;
            int mosaicHeight = (int)(mosaicWidth / imageRatios);
            int yResolution = (int)(height / mosaicHeight);
            int pixelsPerBlock = mosaicHeight * mosaicWidth;
            int picturesPerColumn = yResolution / pixelsPerPicture;
            Image resized = image;
            Pixel[,] colorMap = new Pixel[xResolution, yResolution];

            List<Image> PicturesColor = new List<Image>();
            List<Image> PicturesResized = new List<Image>();
            Dictionary<int, int> nrOfUses = new Dictionary<int, int>();

            int counter = 0;
            int mWidth = pixelsPerPicture * mosaicWidth;
            int mHeight = pixelsPerPicture * mosaicHeight;

            bestForImage = new int[imageList.Count];
            int[] pictureMap = new int[picturesPerRow * picturesPerColumn];
            List<imageAndList> orderedMImg = new List<imageAndList>();
            foreach (Image bitMap in imageList)
            {
                bestForImage[counter] = -1;
                nrOfUses.Add(counter, 0);
                PicturesColor.Add(bitMap);
                PicturesResized.Add(bitMap);
                counter++;
            }

            int setPictureCounter = 0;
            int nrOfPics = picturesPerRow * picturesPerColumn;
            for (int i = 0; i < nrOfPics; i++)
            {
                pictureMap[i] = -1;
            }
            cost = new int[nrOfPics, nrOfPics];
            ordered = new int[nrOfPics, nrOfPics];
            currentImgList = new int[nrOfPics];
            SortedList<int, imageAndList> sortedOrderedMImg = new SortedList<int, imageAndList>();
            for (int x = 0; x < picturesPerRow; x++)
            {
                for (int y = 0; y < picturesPerColumn; y++)
                {
                    int currentNr = x * picturesPerColumn + y;
                    SortedList<int, int> orderedPics = new SortedList<int, int>();
                    for (int i = 0; i < PicturesResized.Count; i++)//Color matchColor in PicturesColor)
                    {
                        int tempVal = matchValue(PicturesColor[i], resized, x * pixelsPerPicture, y * pixelsPerPicture);
                        while (orderedPics.ContainsKey(tempVal))
                            tempVal++;
                        orderedPics.Add(tempVal, i);


                    }
                    for (int i = 0; i < nrOfPics; i++)
                    {
                        int imageN = orderedPics.Values[i];
                        int costOf = orderedPics.Keys[i];
                        cost[currentNr, i] = costOf;
                        ordered[currentNr, i] = imageN;

                    }
                    int image2 = orderedPics.Values[0];
                    int costOf2 = orderedPics.Keys[0];

                    while (sortedOrderedMImg.ContainsKey(costOf2))
                        costOf2++;
                    sortedOrderedMImg.Add(costOf2, new imageAndList(currentNr, orderedPics));
                }

            }


            int outerCounter = 0;
            HashSet<int> imagesUsed = new HashSet<int>();
            while (setPictureCounter < nrOfPics)
            {
                imageAndList imgAndList = sortedOrderedMImg.Values[outerCounter];
                int img = imgAndList.list.Values[0];
                if (imagesUsed.Contains(img))
                {
                    imgAndList.list.RemoveAt(0);
                    if (imgAndList.list.Count > 0)
                    {
                        int key = imgAndList.list.Keys[0];
                        while (sortedOrderedMImg.ContainsKey(key))
                            key++;
                        sortedOrderedMImg.Add(key, imgAndList);
                    }
                }
                else
                {
                    pictureMap[imgAndList.image] = img;
                    imagesUsed.Add(img);
                    setPictureCounter++;
                }
                outerCounter++;
            }

            return pictureMap.ToList().Select(x => imageList[x]).ToList();
        }

    }
}