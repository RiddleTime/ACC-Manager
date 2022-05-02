﻿using ACCSetupApp.SetupParser.SetupRanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.ConversionFactory;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT4
{
    internal class AMRV8VantageGT4 : ICarSetupConversion, ISetupChanger
    {
        public CarModels CarModel => CarModels.Aston_Martin_Vantage_AMR_GT4_2018;

        CarClasses ICarSetupConversion.CarClass => CarClasses.GT4;

        private static readonly double[] casters = new double[] { 10.7, 10.9, 11.1, 11.3, 11.5, 11.6, 11.8, 12.0,
                12.2, 12.4, 12.5, 12.7, 12.9, 13.1, 13.3, 13.4, 13.6, 13.8, 14.0, 14.2, 14.3, 14.5, 14.7, 14.9,
                15.0, 15.2, 15.4, 15.6, 15.7, 15.9, 16.1 };
        private static readonly int[] wheelRateFronts = new int[] { 80000, 90000, 100000, 110000 };
        private static readonly int[] wheelRateRears = new int[] { 70000, 75000, 80000 };

        ITyreSetupChanger ISetupChanger.TyreSetupChanger => new TyreSetupChanger();
        IElectronicsSetupChanger ISetupChanger.ElectronicsSetupChanger => new ElectronicsSetupChanger();
        IMechanicalSetupChanger ISetupChanger.MechanicalSetupChanger => new MechSetupChanger();
        IAeroSetupChanger ISetupChanger.AeroSetupChanger => new AeroSetupChanger();
        IDamperSetupChanger ISetupChanger.DamperSetupChanger => new DamperSetupChanger();

        private class TyreSetupChanger : ITyreSetupChanger
        {
            public SetupDoubleRange TyrePressures => TyrePressuresGT4;
            public SetupDoubleRange CamberFront => new SetupDoubleRange(-4.0, 0.0, 0.1);
            public SetupDoubleRange CamberRear => CamberFront;
            public SetupDoubleRange ToeFront => new SetupDoubleRange(-0.4, 0.4, 0.01);
            public SetupDoubleRange ToeRear => ToeFront;
            public SetupDoubleRange Caster => new SetupDoubleRange(casters);
        }

        private class ElectronicsSetupChanger : IElectronicsSetupChanger
        {
            public SetupIntRange TractionControl => new SetupIntRange(0, 11, 1);
            public SetupIntRange ABS => new SetupIntRange(0, 10, 1);
            public SetupIntRange EcuMap => new SetupIntRange(1, 1, 1);
            public SetupIntRange TractionControlCut => new SetupIntRange(0, 0, 1);
        }

        private class MechSetupChanger : IMechanicalSetupChanger
        {
            public SetupIntRange AntiRollBarFront => new SetupIntRange(0, 3, 1);
            public SetupIntRange AntiRollBarRear => AntiRollBarFront;
            public SetupDoubleRange BrakeBias => new SetupDoubleRange(45.0, 65.0, 0.2);
            public SetupIntRange PreloadDifferential => new SetupIntRange(10, 80, 10);
            public SetupIntRange BrakePower => new SetupIntRange(80, 100, 1);
            public SetupDoubleRange SteeringRatio => new SetupDoubleRange(14, 18, 1);
            public SetupIntRange WheelRateFronts => new SetupIntRange(wheelRateFronts);
            public SetupIntRange WheelRateRears => new SetupIntRange(wheelRateRears);
            public SetupIntRange BumpstopRate => new SetupIntRange(300, 1000, 100);
            public SetupIntRange BumpstopRangeFronts => new SetupIntRange(0, 0, 1);
            public SetupIntRange BumpstopRangeRears => BumpstopRangeFronts;
        }

        private class AeroSetupChanger : IAeroSetupChanger
        {
            public SetupIntRange RideHeightFront => new SetupIntRange(93, 120, 1);
            public SetupIntRange RideHeightRear => new SetupIntRange(97, 130, 1);
            public SetupIntRange BrakeDucts => BrakeDuctsGT3;
            public SetupIntRange Splitter => new SetupIntRange(0, 0, 1);
            public SetupIntRange RearWing => new SetupIntRange(1, 5, 1);
        }

        private class DamperSetupChanger : IDamperSetupChanger
        {
            public SetupIntRange BumpSlow => new SetupIntRange(0, 24, 1);
            public SetupIntRange BumpFast => new SetupIntRange(0, 0, 1);
            public SetupIntRange ReboundSlow => BumpSlow;
            public SetupIntRange ReboundFast => BumpFast;
        }

        AbstractTyresSetup ICarSetupConversion.TyresSetup => new TyreSetup();
        private class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return Math.Round(-4 + 0.1 * rawValue[(int)wheel], 2);
                    case Position.Rear: return Math.Round(-4 + 0.1 * rawValue[(int)wheel], 2);
                    default: return -1;
                }
            }

            public override double Caster(int rawValue)
            {
                return Math.Round(casters[rawValue], 2);
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                return Math.Round(-0.4 + 0.01 * rawValue[(int)wheel], 2);
            }
        }

        IMechanicalSetup ICarSetupConversion.MechanicalSetup => new MechSetup();
        private class MechSetup : IMechanicalSetup
        {
            public int AntiRollBarFront(int rawValue)
            {
                return 0 + rawValue;
            }

            public int AntiRollBarRear(int rawValue)
            {
                return 0 + rawValue;
            }

            public double BrakeBias(int rawValue)
            {
                return Math.Round(45 + 0.2 * rawValue, 2);
            }

            public int BrakePower(int rawValue)
            {
                return 80 + rawValue;
            }

            public int BumpstopRange(List<int> rawValue, Wheel wheel)
            {
                return rawValue[(int)wheel];
            }

            public int BumpstopRate(List<int> rawValue, Wheel wheel)
            {
                return 300 + 100 * rawValue[(int)wheel];
            }

            public int PreloadDifferential(int rawValue)
            {
                return 10 + rawValue * 10;
            }

            public double SteeringRatio(int rawValue)
            {
                return Math.Round(14d + rawValue, 2);
            }

            public int WheelRate(List<int> rawValue, Wheel wheel)
            {
                switch (GetPosition(wheel))
                {
                    case Position.Front: return wheelRateFronts[rawValue[(int)wheel]];
                    case Position.Rear: return wheelRateRears[rawValue[(int)wheel]];
                    default: return -1;
                }
            }
        }

        IDamperSetup ICarSetupConversion.DamperSetup => DefaultDamperSetup;

        IAeroBalance ICarSetupConversion.AeroBalance => new AeroSetup();
        private class AeroSetup : IAeroBalance
        {
            public int BrakeDucts(int rawValue)
            {
                return rawValue;
            }

            public int RearWing(int rawValue)
            {
                return rawValue + 1;
            }

            public int RideHeight(List<int> rawValue, Position position)
            {
                switch (position)
                {
                    case Position.Front: return 93 + rawValue[0];
                    case Position.Rear: return 97 + rawValue[2];
                    default: return -1;
                }
            }

            public int Splitter(int rawValue)
            {
                return rawValue;
            }
        }
    }
}
