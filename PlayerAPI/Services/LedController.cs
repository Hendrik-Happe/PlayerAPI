using PlayerAPI.Models.Config;
using System.Device.Gpio;

namespace PlayerAPI.Services
{
    public class LedController : IDisposable, IGPIOController
    {
        private readonly GpioController? controller;
        private readonly GPIOCofig config;

        public LedController(GPIOCofig config)
        {
            controller = new GpioController();
            this.config = config;

            if (config.PowerLED > 0)
            {
                controller.OpenPin(config.PowerLED, PinMode.Output, PinValue.Low);
            }

            if (config.StatusLED > 0)
            {
                controller.OpenPin(config.StatusLED, PinMode.Output, PinValue.Low);
            }
        }

        public void ShowPlaying()
        {
            TurnOn(config.StatusLED);
        }

        public void HidePlaying()
        {
            TurnOff(config.StatusLED);
        }

        public void ShowPower()
        {
            TurnOn(config.PowerLED);
        }

        public void HidePower()
        {
            TurnOff(config.PowerLED);
        }

        private void TurnOn(int pin)
        {
            if (pin <= 0 || controller == null)
                return;

            controller.Write(pin, PinValue.High);
        }

        private void TurnOff(int pin)
        {
            if (pin <= 0 || controller == null)
                return;

            controller.Write(pin, PinValue.Low);
        }

        public void Dispose()
        {
            HidePower();
            HidePlaying();
            controller?.Dispose();
        }
    }
}
