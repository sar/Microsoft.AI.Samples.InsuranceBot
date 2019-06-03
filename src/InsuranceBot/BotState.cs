using System.Collections.Generic;

namespace InsuranceBot
{
    public class BotState : Dictionary<string, object>
    {
        private const string InsuranceTypeKey = "insuranceType";
        private const string CarTypeKey = "carType";
        private const string CarMakeKey = "carMake";
        private const string CarModelKey = "carModel";
        private const string CarYearKey = "carYear";
        private const string LanguageKey = "language";

        public BotState()
        {
            this[CarTypeKey] = null;
            this[CarMakeKey] = null;
            this[CarModelKey] = null;
            this[CarYearKey] = null;
            this[LanguageKey] = null;
            this[InsuranceTypeKey] = null;
        }

        public string InsuranceType
        {
            get { return (string)this[InsuranceTypeKey]; }
            set { this[InsuranceTypeKey] = value; }
        }

        public string CarType
        {
            get { return (string)this[CarTypeKey]; }
            set { this[CarTypeKey] = value; }
        }

        public string CarMake
        {
            get { return (string)this[CarMakeKey]; }
            set { this[CarMakeKey] = value; }
        }

        public string CarModel
        {
            get { return (string)this[CarModelKey]; }
            set { this[CarModelKey] = value; }
        }

        public int CarYear
        {
            get { return (int)this[CarYearKey]; }
            set { this[CarYearKey] = value; }
        }

        public string Language
        {
            get { return (string)this[LanguageKey]; }
            set { this[LanguageKey] = value; }
        }
    }
}
