using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityHelper.Test
{
    class FL_insurance_sample
    {
        public string policyID { get; set; }
        public StateCode statecode { get; set; }
        public string county { get; set; }
        public string eq_site_limit { get; set; }
        public string hu_site_limit { get; set; }
        public string fl_site_limit { get; set; }
        public string fr_site_limit { get; set; }
        public string tiv_2011 { get; set; }
        public string tiv_2012 { get; set; }
        public string eq_site_deductible { get; set; }
        public string hu_site_deductible { get; set; }
        public string fl_site_deductible { get; set; }
        public string fr_site_deductible { get; set; }
        public string point_latitude { get; set; }
        public string point_longitude { get; set; }
        public string line { get; set; }
        public string construction { get; set; }
        public string point_granularity { get; set; }
    }

    enum StateCode
    {
        [Description("None")]
        None,
        [Description("Florida")]
        FL,
        [Description("Alabama")]
        AL,
        AK,
        AZ,
        AR
    }

    enum StateCodeBad
    {
        None,
        fl_, ak, _a_r
    }
}
