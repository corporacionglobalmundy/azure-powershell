//
// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
// See the License for the specific language governing permissions and
// limitations under the License.
//

// Warning: This code was generated by a tool.
//
// Changes to this file may cause incorrect behavior and will be lost if the
// code is regenerated.

using Microsoft.Azure.Commands.Compute.Automation.Models;
using Microsoft.Azure.Commands.ResourceManager.Common.ArgumentCompleters;
using Microsoft.Azure.Management.Compute.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Azure.Commands.Compute.Automation
{
    [Cmdlet(VerbsCommon.New, ResourceManager.Common.AzureRMConstants.AzureRMPrefix + "ImageConfig", SupportsShouldProcess = true)]
    [OutputType(typeof(PSImage))]
    public partial class NewAzureRmImageConfigCommand : Microsoft.Azure.Commands.ResourceManager.Common.AzureRMCmdlet
    {
        [Parameter(
            Mandatory = false,
            Position = 0,
            ValueFromPipelineByPropertyName = true)]
        [LocationCompleter("Microsoft.Compute/images")]
        public string Location { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 1,
            ValueFromPipelineByPropertyName = true)]
        public Hashtable Tag { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 2,
            ValueFromPipelineByPropertyName = true)]
        public string SourceVirtualMachineId { get; set; }

        [Parameter(
            Mandatory = false,
            Position = 3,
            ValueFromPipelineByPropertyName = true)]
        public ImageOSDisk OsDisk { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true)]
        public ImageDataDisk[] DataDisk { get; set; }

        [Parameter(
            Mandatory = false)]
        public SwitchParameter ZoneResilient { get; set; }

        protected override void ProcessRecord()
        {
            if (ShouldProcess("Image", "New"))
            {
                Run();
            }
        }

        private void Run()
        {
            // SourceVirtualMachine
            Microsoft.Azure.Management.Compute.Models.SubResource vSourceVirtualMachine = null;

            // StorageProfile
            Microsoft.Azure.Management.Compute.Models.ImageStorageProfile vStorageProfile = null;

            if (this.MyInvocation.BoundParameters.ContainsKey("SourceVirtualMachineId"))
            {
                if (vSourceVirtualMachine == null)
                {
                    vSourceVirtualMachine = new Microsoft.Azure.Management.Compute.Models.SubResource();
                }
                vSourceVirtualMachine.Id = this.SourceVirtualMachineId;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey("OsDisk"))
            {
                if (vStorageProfile == null)
                {
                    vStorageProfile = new Microsoft.Azure.Management.Compute.Models.ImageStorageProfile();
                }
                vStorageProfile.OsDisk = this.OsDisk;
            }

            if (this.MyInvocation.BoundParameters.ContainsKey("DataDisk"))
            {
                if (vStorageProfile == null)
                {
                    vStorageProfile = new Microsoft.Azure.Management.Compute.Models.ImageStorageProfile();
                }
                vStorageProfile.DataDisks = this.DataDisk;
            }

            if (vStorageProfile == null)
            {
                vStorageProfile = new Microsoft.Azure.Management.Compute.Models.ImageStorageProfile();
            }
            vStorageProfile.ZoneResilient = this.ZoneResilient.IsPresent;

            var vImage = new PSImage
            {
                Location = this.MyInvocation.BoundParameters.ContainsKey("Location") ? this.Location : null,
                Tags = this.MyInvocation.BoundParameters.ContainsKey("Tag") ? this.Tag.Cast<DictionaryEntry>().ToDictionary(ht => (string)ht.Key, ht => (string)ht.Value) : null,
                SourceVirtualMachine = vSourceVirtualMachine,
                StorageProfile = vStorageProfile,
            };

            WriteObject(vImage);
        }
    }
}
