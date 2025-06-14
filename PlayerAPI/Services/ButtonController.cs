using PlayerAPI.Models.Config;
using System.Collections.Concurrent;
using System.Device.Gpio;

namespace PlayerAPI.Services
{
    public class ButtonController : IDisposable
    {
        private readonly GpioController? controller;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<int, DateTime> lastPress = new();

        // Debounce-Zeit in Millisekunden
        private const int DebounceMs = 200;

        public ButtonController(GPIOCofig config, IVolumeHandler volumeHandler, IMyPlayer myPlayer, ILogger<ButtonController> logger)
        {
            controller = new GpioController();
            this.logger = logger;

            RegisterPin(config.NextButton, (s, e) => myPlayer.Next());
            RegisterPin(config.PreviousButton, (s, e) => myPlayer.Previous());
            RegisterPin(config.VolumeUpButton, (s, e) => volumeHandler.IncreaseVolume());
            RegisterPin(config.VolumeDownButton, (s, e) => volumeHandler.DecreaseVolume());
            RegisterPin(config.PauseButton, (s, e) => myPlayer.Pause());
            RegisterPin(config.PlayButton, (s, e) => myPlayer.Resume());
        }

        private void RegisterPin(int pinNumber, PinChangeEventHandler callback)
        {
            if (pinNumber > 0)
            {
                controller?.OpenPin(pinNumber, PinMode.InputPullUp, PinValue.Low);
                controller?.RegisterCallbackForPinValueChangedEvent(
                    pinNumber,
                    PinEventTypes.Falling,
                    (s, e) =>
                    {
                        var now = DateTime.UtcNow;
                        if (lastPress.TryGetValue(pinNumber, out var last) && (now - last).TotalMilliseconds < DebounceMs)
                        {
                            // Zu schnell, ignorieren (Prellen)
                            return;
                        }
                        lastPress[pinNumber] = now;
                        logger.LogInformation($"Button pressed on pin {pinNumber}");
                        callback(s, e);
                    });
            }
        }

        public void Dispose()
        {
            controller?.Dispose();
        }
    }
}
