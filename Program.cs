using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace slcsp
{
    class Program
    {
        const String PLAN_SILVER = "Silver";
        static void Main(string[] args)
        {
            Console.WriteLine("Reading Files...");
            String header;
            List<Zips> zips = ReadZips();
            List<Plans> plans = ReadPlans();
            List<SLCSP> slcsp = ReadSLCSP(out header);
            DetermineSLCSP(zips, plans, slcsp, header);
            
        }

        static void DetermineSLCSP(List<Zips> zips, List<Plans> plans, List<SLCSP> slcsp, String header) {
            foreach(SLCSP item in slcsp) {
                //make sure all zips are in the same rate area
                String rateAreaTuple;
                if (ZipsAreInSameRateArea(item, zips, out rateAreaTuple)) {
                    item.SLC = GetSLCSP(rateAreaTuple, plans);
                }
                else { //ambiguous 
                    item.SLC = String.Empty;
                }
            }
            WriteSLCSP(header, slcsp);
        }

        static void WriteSLCSP(String header, List<SLCSP> slcsp) {
            using (StreamWriter writer = new StreamWriter("slcsp_answer.csv", false)) {
                writer.WriteLine(header);
                slcsp.ForEach(entry => writer.WriteLine(entry.ZipCode + "," + entry.SLC));
            }
        }

        static String GetSLCSP(string rateAreaTuple, List<Plans> plans) {            
            String stateTup = rateAreaTuple.Split(" ")[0];
            String rateAreaTup = rateAreaTuple.Split(" ")[1];
            return plans.Where(p => p.State == stateTup && p.RateArea.ToString() == rateAreaTup && 
                p.MetalLevel == PLAN_SILVER).OrderBy(o => o.Rate).GroupBy(x => x.Rate).Select(r => String.Format
                ("{0:0.00}", r.First().Rate)).Skip(1).FirstOrDefault();                
            //We use GroupBy in the above lambda spread as a 'distinct' 
            //select into the List and order asc. The second element is our SLCSP so Skip(1)
            //then take the first after or empty if ambiguous

            
        }

        static bool ZipsAreInSameRateArea(SLCSP item, List<Zips> zips, out String rateAreaTuple) {
            //get all the zips from the zip file that match 'item.zip'
            List<Zips> zip = zips.Where(z => z.ZipCode == item.ZipCode).ToList<Zips>();
            //if any of the zips gathered are in a different rate area (and the zip was actually found)
            if (zip.Any(z => z.RateArea != zip[0].RateArea) && zip.Count() > 0) {
                //then result is ambiguous
                rateAreaTuple = String.Empty;
                return false;
            }            
            //else all the same zips retrieved are in the same rate area
            rateAreaTuple = zip[0].State + " " + zip[0].RateArea;
            return true;
        }

        static List<SLCSP> ReadSLCSP(out String header) {
            List<SLCSP> slcsp = new List<SLCSP>();
            using (var slcpReader = new StreamReader("slcsp.csv")) {
                header = slcpReader.ReadLine(); //header
                while(!slcpReader.EndOfStream) {
                    slcsp.Add(new SLCSP(slcpReader.ReadLine().Split(',')));
                }
            }
            return slcsp;
        }
        static List<Plans> ReadPlans() {
        List<Plans> plans = new List<Plans>();
            using (var planReader = new StreamReader("plans.csv")) {
                planReader.ReadLine(); //toss the headers
                while(!planReader.EndOfStream) {
                    plans.Add(new Plans(planReader.ReadLine().Split(',')));
                }
            }
            return plans;
        }
        static List<Zips> ReadZips() {
            List<Zips> zips = new List<Zips>();
            using (var zipReader = new StreamReader("zips.csv")) {
                zipReader.ReadLine(); //toss headers
                while(!zipReader.EndOfStream) {
                    zips.Add(new Zips(zipReader.ReadLine().Split(',')));
                }
            }
            return zips;
        }
    }

    public class SLCSP {
        public SLCSP() {

        }

        public SLCSP(String zipCode, String slcsp) {
            _zipCode = zipCode;
            _slcsp = slcsp;
        }

        public SLCSP(String[] slcsp) {
            _zipCode = slcsp[0];
            _slcsp = slcsp[1];
        }

        public String ZipCode {
            get {return _zipCode;}
            set {_zipCode = value;}
        }
        String _zipCode;

        public String SLC {
            get {return _slcsp;}
            set {_slcsp = value;}
        }
        String _slcsp;
    }

    public class Zips {
        public Zips() {

        }

        public Zips(String zipCode, String state, String countyCode, String name, Int32 rateArea) {
            _zipCode = zipCode;
            _state = state;
            _countyCode = countyCode;
            _name = name;
            _rateArea = rateArea;
        }

        public Zips(String[] zip) {
            Int32 result;
            _zipCode = zip[0];
            _state = zip[1];
            _countyCode = zip[2];
            _name = zip[3];
            _rateArea = Int32.TryParse(zip[4], out result) ? result : 0;
        }

        public String ZipCode {
            get{return _zipCode;}
            set{_zipCode = value;}
        }
        String _zipCode;

        public String State {
            get {return _state;}
            set {_state = value;}
        }
        String _state;

        public String CountyCode {
            get {return _countyCode;}
            set {_countyCode = value;}
        }
        String _countyCode;

        public String Name {
            get {return _name;}
            set {_name = value;}
        }
        String _name;

        public Int32 RateArea {
            get {return _rateArea;}
            set {_rateArea = value;}
        }
        Int32 _rateArea;
    }

    public class Plans {
        public Plans() {

        }

        public Plans(String id, String state, String metalLevel, Decimal rate, Int32 rateArea) {
            _id = id;
            _state = state;
            _metalLevel = metalLevel;
            _rate = rate;
            _rateArea = rateArea;
        }

        public Plans(String[] line) {
            Decimal _rateResult;
            Int32 _rateAreaResult;
            _id = line[0];
            _state = line[1];
            _metalLevel = line[2];
            _rate = Decimal.TryParse(line[3], out _rateResult) ? _rateResult : 0;
            _rateArea = Int32.TryParse(line[4], out _rateAreaResult) ? _rateAreaResult : 0;
        }

        public String ID {
            get { return _id;}
            set {_id = value;}
        }
        String _id;

        public String State {
            get {return _state;}
            set {_state = value;}
        }
        String _state;

        public String MetalLevel {
            get {return _metalLevel;}
            set {_metalLevel = value;}
        }
        String _metalLevel;

        public Decimal Rate {
            get {return _rate;}
            set {_rate = value;}
        }
        Decimal _rate;

        public Int32 RateArea {
            get {return _rateArea;}
            set {_rateArea = value;}
        }
        Int32 _rateArea;

    }
}
