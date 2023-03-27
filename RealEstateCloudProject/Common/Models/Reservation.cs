using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [DataContract]
    public class Reservation
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string EstateId { get; set; }

        [DataMember]
        public string EstateName { get; set; }
        [DataMember] 
        public string EstatePlace { get; set; }
        [DataMember]
        public int EstateRentingPrice { get; set; }
        [DataMember]
        public string DateFrom { get; set; }
        [DataMember]
        public string DateTo { get; set; }

        public Reservation()
        {
        }

        public Reservation(int id, string estateId, string estateName, string estatePlace, int estateRentingPrice, string dateFrom, string dateTo)
        {
            Id = 0;
            EstateId = estateId;
            EstateName = estateName;
            EstatePlace = estatePlace;
            EstateRentingPrice = estateRentingPrice;
            DateFrom = dateFrom;
            DateTo = dateTo;
        }
    }
}
