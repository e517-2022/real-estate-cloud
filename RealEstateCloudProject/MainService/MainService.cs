using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Common.Tables;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace MainService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MainService : StatefulService,IMainService
    {
        IReliableDictionary<int, RealEstate> realEstateDict;
        IReliableDictionary<int, Reservation> reservationDict;
        TableHelper tableHelper= new TableHelper();
        public MainService(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<bool> AddNewEstate(RealEstate realEstate)
        {
            
            var stateManager = this.StateManager;
            bool result = true;

            realEstate.Id =Guid.NewGuid().GetHashCode();


            realEstateDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, RealEstate>>("RealEstateData");
            using (var  t = stateManager.CreateTransaction())
            {
                result = await realEstateDict.TryAddAsync(t, realEstate.Id, realEstate);
                await t.CommitAsync();
            }

            if (result == false)
            {
                return false;
            }

            List<RealEstate> estatesPS = await GetEstates();
            FabricClient fabricClient = new System.Fabric.FabricClient();
            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/RealEstateCloudProject/PubSub"))).Count;
            int index = 0;

            for (int i = 0; i < partitionsNumber; i++)
            {
                var proxy = ServiceProxy.Create<IPubSub>(
                new Uri("fabric:/RealEstateCloudProject/PubSub"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                );

                bool tempPublish = await proxy.NewEstatePublish(estatesPS);

                if (tempPublish == false)
                {
                    return tempPublish;
                }

                index++;
            }

            return result;

            
        }

        public async Task<List<RealEstate>> GetEstates()
        {

            List<RealEstate> estates = new List<RealEstate>();

           

                realEstateDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, RealEstate>>("RealEstateData");
                using (var tx = this.StateManager.CreateTransaction())
                {
                    var enumerator = (await realEstateDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                    while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                    {
                        estates.Add(enumerator.Current.Value);
                    }
                }

                return estates;
               
            

        }

        public async Task<List<Reservation>> GetReservations()
        {

            List<Reservation> res = new List<Reservation>();



            reservationDict = await this.StateManager.GetOrAddAsync<IReliableDictionary<int, Reservation>>("ReservationData");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerator = (await reservationDict.CreateEnumerableAsync(tx)).GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    res.Add(enumerator.Current.Value);
                }
            }

            return res;



        }

        public async Task<List<Reservation>> GetReservationsFromTable()
        {

            List<Reservation> res = new List<Reservation>();



            res = await tableHelper.GetReservations();

            return res;



        }


        public async Task<bool> AddNewReservation(Reservation reservation)
        {

            var stateManager = this.StateManager;
            reservation.Id = Guid.NewGuid().GetHashCode();
            bool result = true;

            List<RealEstate> estates =await GetEstates();
            List<Reservation> reservations = await GetReservations();

            if(reservation.DateTo==null || reservation.DateFrom == null || DateTime.Parse(reservation.DateFrom)>DateTime.Parse(reservation.DateTo) || DateTime.Parse(reservation.DateFrom)<DateTime.Now)
            {
                return false;
            }

            if (reservations != null)
            {
                foreach(Reservation r in reservations)
                {
                    if (r.EstateId == reservation.EstateId)
                    {
                        if(DateTime.Parse(r.DateFrom)<= DateTime.Parse(reservation.DateFrom) && DateTime.Parse(r.DateTo)>= DateTime.Parse(reservation.DateFrom))
                        {
                            return false;
                        }
                    }
                    else if(DateTime.Parse(r.DateFrom)<= DateTime.Parse(reservation.DateTo) && DateTime.Parse(r.DateTo)>= DateTime.Parse(reservation.DateTo))
                    {
                        return false;
                    }
                } 
              
            }

          



            reservationDict = await stateManager.GetOrAddAsync<IReliableDictionary<int, Reservation>>("ReservationData");
            using (var t = stateManager.CreateTransaction())
            {
                result = await reservationDict.TryAddAsync(t, reservation.Id, reservation);
                await tableHelper.AddReservation(reservation);
                await t.CommitAsync();
            }

            if (result == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //izmenjeno
            return this.CreateServiceRemotingReplicaListeners();
        }

    

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");
           

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
