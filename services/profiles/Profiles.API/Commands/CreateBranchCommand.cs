using EasyGas.Services.Core.Commands;
using EasyGas.Services.Profiles.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyGas.Services.Profiles.Commands
{
    public class CreateBranchCommand : CommandBase
    {
        public Branch Branch;
        public bool IsUpdate;

        public CreateBranchCommand(Branch model, bool isUpdate)
        {
            Branch = CreateBranchFromModel(model);
            IsUpdate = isUpdate;
        }

        private Branch CreateBranchFromModel(Branch model)
        {
            return model;
        }

        protected override IEnumerable<string> OnValidation()
        {
            if (Branch == null)
            {
                yield return "Invalid or no payload received";
            }
            else
            {
                if (IsUpdate)
                {
                    if (Branch.Id <= 0)
                    {
                        yield return "Invalid branch";
                    }
                }
                if (String.IsNullOrEmpty(Branch.Name))
                {
                    yield return "Name is missing";
                }
                if (Branch.TenantId <= 0)
                {
                    yield return "Tenant is missing";
                }
            }
        }
    }
}

