using System; 
using System.ServiceModel; // Importa funcionalidades para tratamento de serviços e exceções do WCF.
using Microsoft.Xrm.Sdk; // Importa interfaces e classes do SDK do Dynamics 365.

namespace PluginTutorialMicrosoft
{
    public class FollowupPlugin : IPlugin // Define a classe FollowupPlugin que implementa a interface IPlugin.
    {
        public void Execute(IServiceProvider serviceProvider) // Método principal executado pelo Dynamics 365 quando o plugin é disparado.
        {
            //Obtendo o serviço de tracing:
            ITracingService tracingService  = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtém o serviço de rastreamento para registrar logs e mensagens de depuração.

            //Obtendo o contexto de execução do service provider
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            // Obtém o contexto de execução do plugin, contendo informações sobre a execução atual.

            // A coleção InputParameters contém todos os dados passados ​​na solicitação de mensagem.
            if(context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            // Verifica se existe um parâmetro chamado "Target" e se ele é uma entidade do Dynamics.
            {
                // Obtendo a entidade de destino a partir dos parâmetros de entrada.
                Entity entity =(Entity)context.InputParameters["Target"];
                // Obtém a entidade que disparou o plugin.

                // Obtendo a instância IOrganizationService que você precisará para
                // chamadas de serviço web.
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                // Obtém a fábrica de serviços para criar instâncias do serviço de organização.

                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
                // Cria uma instância do serviço de organização usando o ID do usuário que disparou o plugin.

                try
                {
                    //Lógica de negócio do plug-in vai aqui!
                    // Criando uma atividade de tarefa para acompanhar o cliente da conta em 7 dias.

                    Entity followUp = new Entity("task");
                    // Cria uma nova entidade do tipo "task" (tarefa).

                    followUp["subject"] = "Envie um e-mail para o novo cliente.";
                    // Define o assunto da tarefa.

                    followUp["description"] = "Acompanhe o cliente. Verifique se há novos problemas que precisam ser resolvidos.";
                    // Define a descrição da tarefa.

                    followUp["scheduledstart"] = DateTime.Now.AddDays(7);
                    // Define a data de início da tarefa para 7 dias após a data atual.

                    followUp["scheduledend"] = DateTime.Now.AddDays(7);
                    // Define a data de término da tarefa para 7 dias após a data atual.

                    followUp["category"] = context.PrimaryEntityName;
                    // Define a categoria da tarefa como o nome da entidade principal que disparou o plugin.

                    // Consultando a conta na atividade da tarefa.
                    if (context.OutputParameters.Contains("id"))
                    // Verifica se existe um parâmetro de saída chamado "id".
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        // Obtém o ID da entidade relacionada.

                        string regardingobjectType = "account";
                        // Define o tipo da entidade relacionada como "account" (conta).

                        followUp["regardingobjectid"] = new EntityReference(regardingobjectType, regardingobjectid);
                        // Relaciona a tarefa à conta usando o ID obtido.
                    }

                    //Criando a tarefa no Dynamics
                    tracingService.Trace("FollowUpPlugin: Criando a atividade da tarefa");
                    // Registra uma mensagem de rastreamento indicando que a tarefa está sendo criada.

                    service.Create(followUp);
                    // Cria a tarefa no Dynamics 365 usando o serviço de organização.
                }
                catch (FaultException<OrganizationServiceFault> ex)
                // Captura exceções específicas do serviço de organização.
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    // Registra a exceção no serviço de rastreamento.

                    throw new InvalidPluginExecutionException("Um erro ocorreu no plugin FollowUpPlugin", ex);
                    // Lança uma exceção personalizada para o Dynamics 365.
                }
                catch (Exception ex)
                // Captura outras exceções genéricas.
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    // Registra a exceção genérica no serviço de rastreamento.
                }
            }
        }
    }
}
