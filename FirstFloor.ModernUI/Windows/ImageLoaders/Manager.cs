// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from FirstFloor.ModernUI INC. team.
//  
// Copyrights (c) 2014 FirstFloor.ModernUI INC. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FirstFloor.ModernUI.Windows.ImageLoaders
{
    internal sealed class Manager
    {
        private static readonly Manager instance = new Manager();
        private readonly DrawingImage _errorThumbnail;
        private readonly Dictionary<Image, LoadImageRequest> _imagesLastRunningTask = new Dictionary<Image, LoadImageRequest>();

        private readonly Stack<LoadImageRequest> _loadNormalStack = new Stack<LoadImageRequest>();
        private readonly Stack<LoadImageRequest> _loadThumbnailStack = new Stack<LoadImageRequest>();

        private readonly AutoResetEvent _loaderThreadNormalSizeEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _loaderThreadThumbnailEvent = new AutoResetEvent(false);

        private readonly TransformGroup _loadingAnimationTransform;
        private readonly DrawingImage _loadingImage;
        internal Thread _loaderThreadForNormalSize;
        internal Thread _loaderThreadForThumbnails;

        private Manager()
        {
            _loaderThreadForThumbnails = new Thread(LoaderThreadThumbnails) {IsBackground = true, Priority = ThreadPriority.BelowNormal};
            _loaderThreadForThumbnails.Start();

            _loaderThreadForNormalSize = new Thread(LoaderThreadNormalSize) {IsBackground = true, Priority = ThreadPriority.BelowNormal};
            _loaderThreadForNormalSize.Start();

            var resourceDictionary = new ResourceDictionary {Source = new Uri("FirstFloor.ModernUI;component/Assets/ImageLoader.xaml", UriKind.Relative)};
            _loadingImage = resourceDictionary["ImageLoading"] as DrawingImage;

            _loadingImage.Freeze();
            _errorThumbnail = resourceDictionary["ImageError"] as DrawingImage;
            _errorThumbnail.Freeze();

            var scaleTransform = new ScaleTransform(0.5, 0.5);
            var skewTransform = new SkewTransform(0, 0);
            var rotateTransform = new RotateTransform(0);
            var translateTransform = new TranslateTransform(0, 0);

            var group = new TransformGroup();
            group.Children.Add(scaleTransform);
            group.Children.Add(skewTransform);
            group.Children.Add(rotateTransform);
            group.Children.Add(translateTransform);

            var doubleAnimation = new DoubleAnimation(0, 359, new TimeSpan(0, 0, 0, 1)) {RepeatBehavior = RepeatBehavior.Forever};

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);

            _loadingAnimationTransform = group;
        }

        public static Manager Instance { get { return instance; } }

        public void LoadImage(string source, Image image)
        {
            var loadTask = new LoadImageRequest {Image = image, Source = source};

            BeginLoading(image, loadTask);

            lock(_loadThumbnailStack)
            {
                _loadThumbnailStack.Push(loadTask);
            }

            _loaderThreadThumbnailEvent.Set();
        }

        private void BeginLoading(Image image, LoadImageRequest loadTask)
        {
            lock(_imagesLastRunningTask)
            {
                if(_imagesLastRunningTask.ContainsKey(image))
                {
                    _imagesLastRunningTask[image].IsCanceled = true;
                    _imagesLastRunningTask[image] = loadTask;
                }
                else
                {
                    _imagesLastRunningTask.Add(image, loadTask);
                }
            }

            image.Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                Loader.SetIsLoading(image, true);

                if(image.RenderTransform == Transform.Identity)
                {
                    if(Loader.GetDisplayWaitingAnimationDuringLoading(image))
                    {
                        image.Source = _loadingImage;
                        image.RenderTransformOrigin = new Point(0.5, 0.5);
                        image.RenderTransform = _loadingAnimationTransform;
                    }
                }
            }));
        }

        private void EndLoading(Image image, ImageSource imageSource, LoadImageRequest loadTask, bool markAsFinished)
        {
            lock(_imagesLastRunningTask)
            {
                if(_imagesLastRunningTask.ContainsKey(image))
                {
                    if(_imagesLastRunningTask[image].Source != loadTask.Source)
                    {
                        return;
                    }

                    if(markAsFinished)
                    {
                        _imagesLastRunningTask.Remove(image);
                    }
                }
                else
                {
                    Debug.WriteLine("EndLoading() - unexpected condition: there is no running task for this image!");
                }

                image.Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    if(image.RenderTransform == _loadingAnimationTransform)
                    {
                        image.RenderTransform = Transform.Identity;
                    }

                    if(Loader.GetErrorDetected(image) && Loader.GetDisplayErrorThumbnailOnError(image))
                    {
                        imageSource = _errorThumbnail;
                    }

                    image.Source = imageSource;

                    if(markAsFinished)
                    {
                        Loader.SetIsLoading(image, false);
                    }
                }));
            }
        }

        private ImageSource GetBitmapSource(LoadImageRequest loadTask)
        {
            var image = loadTask.Image;
            var source = loadTask.Source;

            ImageSource imageSource = null;

            if(!string.IsNullOrWhiteSpace(source))
            {
                Stream imageStream = null;

                var sourceType = SourceType.LocalDisk;

                if(source.StartsWith("http"))
                {
                    sourceType = SourceType.ExternalResource;
                }
                //image.Dispatcher.Invoke(new ThreadStart(delegate { sourceType = Loader.GetSourceType(image); }));

                try
                {
                    if(loadTask.Stream == null)
                    {
                        var loader = LoaderFactory.CreateLoader(sourceType);
                        imageStream = loader.Load(source);
                        loadTask.Stream = imageStream;
                    }
                    else
                    {
                        imageStream = new MemoryStream();
                        loadTask.Stream.Position = 0;
                        loadTask.Stream.CopyTo(imageStream);
                        imageStream.Position = 0;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                if(imageStream != null)
                {
                    try
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = imageStream;
                        bitmapImage.EndInit();
                        imageSource = bitmapImage;
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                if(imageSource == null)
                {
                    image.Dispatcher.BeginInvoke(new ThreadStart(() => Loader.SetErrorDetected(image, true)));
                }
                else
                {
                    imageSource.Freeze();

                    image.Dispatcher.BeginInvoke(new ThreadStart(() => Loader.SetErrorDetected(image, false)));
                }
            }
            else
            {
                image.Dispatcher.BeginInvoke(new ThreadStart(() => Loader.SetErrorDetected(image, false)));
            }

            return imageSource;
        }

        private void LoaderThreadThumbnails()
        {
            do
            {
                _loaderThreadThumbnailEvent.WaitOne();

                LoadImageRequest loadTask = null;

                do
                {
                    lock(_loadThumbnailStack)
                    {
                        loadTask = _loadThumbnailStack.Count > 0 ? _loadThumbnailStack.Pop() : null;
                    }

                    if(loadTask != null && !loadTask.IsCanceled)
                    {
                        var bitmapSource = GetBitmapSource(loadTask);

                        EndLoading(loadTask.Image, bitmapSource, loadTask, false);

                        lock(_loadNormalStack)
                        {
                            _loadNormalStack.Push(loadTask);
                        }

                        _loaderThreadNormalSizeEvent.Set();
                    }
                }
                while(loadTask != null);
            }
            while(true);
        }

        private void LoaderThreadNormalSize()
        {
            do
            {
                _loaderThreadNormalSizeEvent.WaitOne();

                LoadImageRequest loadTask = null;

                do
                {
                    lock(_loadNormalStack)
                    {
                        loadTask = _loadNormalStack.Count > 0 ? _loadNormalStack.Pop() : null;
                    }

                    if(loadTask != null && !loadTask.IsCanceled)
                    {
                        var bitmapSource = GetBitmapSource(loadTask);
                        EndLoading(loadTask.Image, bitmapSource, loadTask, true);
                    }
                }
                while(loadTask != null);
            }
            while(true);
        }

        internal class LoadImageRequest
        {
            public bool IsCanceled { get; set; }
            public string Source { get; set; }
            public Stream Stream { get; set; }
            public Image Image { get; set; }
        }
    }
}