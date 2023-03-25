using System;
using System.Runtime.Serialization;

namespace Common.Models
{
    [DataContract]
    public class RealEstate
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Place { get; set; }

        [DataMember]
        public int RentingPrice { get; set; }

        public RealEstate(int id, string name, string place, int rentingPrice)
        {
            Id = 0;
            Name = name;
            Place = place;
            RentingPrice = rentingPrice;
        }

        public RealEstate()
        {
        }
    }
}
