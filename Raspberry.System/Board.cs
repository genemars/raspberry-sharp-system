#region References

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#endregion

namespace Raspberry
{
    /// <summary>
    /// Represents the Raspberry Pi mainboard.
    /// </summary>
    /// <remarks>Version and revisions are based on <see cref="http://raspberryalphaomega.org.uk/2013/02/06/automatic-raspberry-pi-board-revision-detection-model-a-b1-and-b2/"/>.</remarks>
    public class Board
    {
        #region Fields

        private static readonly Lazy<Board> board = new Lazy<Board>(LoadBoard);

        private readonly Dictionary<string, string> settings;
        private readonly Lazy<Model> model;
        private readonly Lazy<ConnectorPinout> connectorPinout;

        #endregion

        #region Instance Management

        private Board(Dictionary<string, string> settings)
        {
            model = new Lazy<Model>(LoadModel);
            connectorPinout = new Lazy<ConnectorPinout>(LoadConnectorPinout);
            this.settings = settings;
            Console.WriteLine("connectorPinout = " + connectorPinout);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current mainboard configuration.
        /// </summary>
        public static Board Current
        {
            get { return board.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a Raspberry Pi.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a Raspberry Pi; otherwise, <c>false</c>.
        /// </value>
        public bool IsRaspberryPi
        {
            get
            {
                return Processor != Processor.Unknown;
            }
        }

        /// <summary>
        /// Gets the processor name.
        /// </summary>
        /// <value>
        /// The name of the processor.
        /// </value>
        public string ProcessorName
        {
            get
            {
                string hardware;
                return settings.TryGetValue("Hardware", out hardware) ? hardware : null;
            }
        }

        /// <summary>
        /// Gets the processor.
        /// </summary>
        /// <value>
        /// The processor.
        /// </value>
        public Processor Processor
        {
            get
            {
                Processor processor;
                if (Enum.TryParse(ProcessorName, true, out processor))
                {
                    // Check to see if we're dealing with a Pi 4 Model B
                    // The Pi 4 Model B currently lies to us and tells us that it's a BCM2835
                    if (processor == Processor.Bcm2835 && Model == Model.Pi4)
                    {
                        processor = Processor.Bcm2711;
                    }
                    Console.WriteLine(processor);
                    return processor;
                }

                Console.WriteLine("Processor.Unknown");
                return  Processor.Unknown;
            }
        }

        /// <summary>
        /// Gets the board firmware version.
        /// </summary>
        public int Firmware
        {
            get
            {
                string revision;
                int firmware;
                if (settings.TryGetValue("Revision", out revision)
                    && !string.IsNullOrEmpty(revision)
                    && int.TryParse(revision, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out firmware))
                    return firmware;

                return 0;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public string SerialNumber
        {
            get {
                string serial;
                if (settings.TryGetValue("Serial", out serial)
                    && !string.IsNullOrEmpty(serial))
                    return serial;

                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Raspberry Pi board is overclocked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Raspberry Pi is overclocked; otherwise, <c>false</c>.
        /// </value>
        public bool IsOverclocked
        {
            get
            {
                var firmware = Firmware;
                return (firmware & 0xFFFF0000) != 0;
            }
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public Model Model
        {
            get { return model.Value; }
        }

        /// <summary>
        /// Gets the connector revision.
        /// </summary>
        /// <value>
        /// The connector revision.
        /// </value>
        /// <remarks>See <see cref="http://raspi.tv/2014/rpi-gpio-quick-reference-updated-for-raspberry-pi-b"/> for more information.</remarks>
        public ConnectorPinout ConnectorPinout
        {
            get { return connectorPinout.Value; }
        }

        #endregion

        #region Private Helpers

        private static Board LoadBoard()
        {
            try
            {
                const string filePath = "/proc/cpuinfo";

                var cpuInfo = File.ReadAllLines(filePath);
                var settings = new Dictionary<string, string>();
                var suffix = string.Empty;

                foreach(var l in cpuInfo)
                {
                    var separator = l.IndexOf(':');

                    if (!string.IsNullOrWhiteSpace(l) && separator > 0)
                    {
                        var key = l.Substring(0, separator).Trim();
                        var val = l.Substring(separator + 1).Trim();
                        if (string.Equals(key, "processor", StringComparison.InvariantCultureIgnoreCase))
                            suffix = "." + val;

                        settings.Add(key + suffix, val);
                        Console.WriteLine("setting.add = " + key + " + " + suffix + ", " + val);
                    }
                    else
                        suffix = "";
                }

                return new Board(settings);
            }
            catch
            {
                return new Board(new Dictionary<string, string>());
            }
        }

        private Model LoadModel()
        {
            var firmware = Firmware;
            Console.WriteLine("Firmware = {0:x}", firmware);
            Console.WriteLine("Firmware & 0xFFFF = {0:x}", firmware & 0xFFFF);
            
            switch (firmware & 0xFFFF)
            {
                case 0x2:
                case 0x3:
                    Console.WriteLine("Model.BRev1");
                    return Model.BRev1;

                case 0x4:
                case 0x5:
                case 0x6:
                case 0xd:
                case 0xe:
                case 0xf:
                    Console.WriteLine("Model.BRev2");
                    return Model.BRev2;

                case 0x7:
                case 0x8:
                case 0x9:
                    Console.WriteLine("Model.A");
                    return Model.A;

                case 0x10:
                    Console.WriteLine("Model.BPlus1");
                    return Model.BPlus;

                case 0x11:
                    Console.WriteLine("Model.ComputeModule");
                    return Model.ComputeModule;

                case 0x12:
                    Console.WriteLine("Model.APlus");
                    return Model.APlus;

                case 0x1040:
                case 0x1041:
                    Console.WriteLine("Model.B2");
                    return Model.B2;

                case 0x0092:
                case 0x0093:
                    Console.WriteLine("Model.Zero");
                    return Model.Zero;

                case 0x2082:
                    Console.WriteLine("Model.B3");
                    return Model.B3;
                case 0x20A0:
                    Console.WriteLine("Model.ComputeModule3");
                    return Model.ComputeModule3;

                case 0x03111:
                case 0x03112:
                case 0x03114:
                    Console.WriteLine("Model.Pi4");
                    return Model.Pi4;

                default:
                    Console.WriteLine("Model.Unknown");
                    return Model.Unknown;
            }
        }

        private ConnectorPinout LoadConnectorPinout()
        {
            switch (Model)
            {
                case Model.BRev1:
                    Console.WriteLine("ConnectorPinout.Rev1");
                    return ConnectorPinout.Rev1;

                case Model.BRev2:
                case Model.A:
                    Console.WriteLine("ConnectorPinout.Rev2");
                    return ConnectorPinout.Rev2;

                case Model.BPlus:
                case Model.ComputeModule:
                case Model.APlus:
                case Model.B2:
                case Model.Zero:
                case Model.B3:
                case Model.ComputeModule3:
                case Model.Pi4:
                    Console.WriteLine("ConnectorPinout.Plus");
                    return ConnectorPinout.Plus;

                default:
                    Console.WriteLine("ConnectorPinout.Unknown");
                    return ConnectorPinout.Unknown;
            }
        }

        #endregion
    }
}
