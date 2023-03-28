using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using Spire.Email;
using Spire.Email.Pop3;

namespace EmailService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class EmailService : StatefulService
    {
        public EmailService(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
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
                await CheckMailsRecived();
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }



        public async Task CheckMailsRecived()
        {
            try
            {
                Pop3Client pop = new Pop3Client();
                pop.Host = "pop.gmail.com";
                pop.Username = "cloudreal172022@gmail.com";
                pop.Password = "creihiptvkydhawy";
                pop.Port = 995;
                pop.EnableSsl = true;
                pop.Connect();
                int numberofMails = pop.GetMessageCount();


                using (var tx = this.StateManager.CreateTransaction())
                {
                    for (int i = 1; i <= numberofMails; i++)
                    {
                        MailMessage message = pop.GetMessage(i);
                        string[] mail = message.BodyText.Split(';');
                        try
                        {
                            Reservation res = new Reservation();
                            res.Id = Guid.NewGuid().GetHashCode();
                            string id = mail[0];
                            string name = mail[1];
                            string place = mail[2];
                           
                            int price = Int32.Parse(mail[3]);
                            string from = mail[4];
                            string to = mail[5];




                            FabricClient fabricClient = new System.Fabric.FabricClient();
                            int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/RealEstateCloudProject/MainService"))).Count;
                            int index = 0;

                            for (int j = 0; j < partitionsNumber; j++)
                            {
                                var proxy = ServiceProxy.Create<IMainService>(
                                new Uri("fabric:/RealEstateCloudProject/MainService"),
                                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                                );

                                Reservation rsr = new Reservation(res.Id, id, name, place, price, from, to);
                                rsr.Id = res.Id;
                                bool result=await proxy.AddNewReservation(rsr);

                                index++;
                            }

                        }
                        catch
                        {
                            ServiceEventSource.Current.Message("FAiled");
                        }
                    }
                    await tx.CommitAsync();
                }
                pop.DeleteAllMessages();
                pop.Disconnect();

            }
            catch
            {
                ServiceEventSource.Current.Message("Email servis trenutno ne radi");

            }

        }

    }
}
