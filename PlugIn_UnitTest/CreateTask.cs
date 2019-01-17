using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlugIn_UnitTest
{
    public class CreateTask : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                if (entity.LogicalName != "account")
                    return;
                try
                {
                    //string recordName = (string)entity.Attributes["name"];
                    // Create a task activity to follow up with the account customer
                    Entity task = new Entity("task");

                    task["subject"] = "Send e-mail to the new customer.";
                    task["description"] = "Follow up with the customer. Check if there are any new issues that need resolution.";
                    task["scheduledstart"] = DateTime.Now;
                    task["scheduledend"] = DateTime.Now.AddDays(2);
                    task["category"] = context.PrimaryEntityName;
                    // Refer to the contact in the task activity.
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectid = new Guid(context.OutputParameters["id"].ToString());
                        string regardingobjectidType = entity.LogicalName;
                        task["regardingobjectid"] = new EntityReference(regardingobjectidType, regardingobjectid);
                    }
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    // Create the followup activity
                    service.Create(task);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException(ex.Message);
                }
            }
        }
    }
}
