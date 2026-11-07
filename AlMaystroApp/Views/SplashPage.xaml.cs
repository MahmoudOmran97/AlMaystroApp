using AlMaystroApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMaystroApp.Views;

    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();
            BackgroundColor = Color.FromArgb("#333333");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RunSplashAnimations();
        }

        private async Task RunSplashAnimations()
        {
            await Task.Delay(0);

            // ✅ Logo entrance
            await Task.WhenAll(
                LogoImage.FadeTo(1, 700, Easing.CubicOut),
                LogoImage.ScaleTo(1.05, 600, Easing.CubicOut)
            );
            await LogoImage.ScaleTo(1.0, 200, Easing.BounceOut);

            await Task.Delay(200);

            // ✅ Contact stack
            await Task.WhenAll(
                ContactStack.FadeTo(1, 600, Easing.CubicOut),
                ContactStack.TranslateTo(0, 0, 600, Easing.CubicOut)
            );

            // ✅ واتساب أيقونة - Bounce Animation
            await Task.WhenAll(
                WaIcon.FadeTo(1, 300, Easing.CubicOut),
                WaIcon.ScaleTo(1.3, 300, Easing.CubicOut)
            );
            await WaIcon.ScaleTo(0.9, 150, Easing.CubicIn);
            await WaIcon.ScaleTo(1.1, 150, Easing.CubicOut);
            await WaIcon.ScaleTo(1.0, 100, Easing.CubicIn);

            // ✅ نبضة متكررة كل ثانيتين
            _ = PulseWhatsApp();

            await Task.Delay(150);

            // ✅ Tagline
            await Task.WhenAll(
                TaglineLabel.FadeTo(1, 500, Easing.CubicOut),
                TaglineLabel.TranslateTo(0, 0, 500, Easing.CubicOut)
            );

            await Task.Delay(2000);
            await MainStack.FadeTo(0, 400, Easing.CubicIn);
            Application.Current!.MainPage = new AppShell();
        }

        // ✅ نبضة مستمرة للأيقونة
        private async Task PulseWhatsApp()
        {
            while (true)
            {
                await WaIcon.ScaleTo(1.15, 600, Easing.SinInOut);
                await WaIcon.ScaleTo(1.0, 600, Easing.SinInOut);
                await Task.Delay(500);
            }
        }
    }


