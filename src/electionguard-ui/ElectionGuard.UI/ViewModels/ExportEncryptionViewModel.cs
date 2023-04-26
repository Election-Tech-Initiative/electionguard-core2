using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.ViewModels
{
    public partial class EncryptionPackageExportViewModel
        : BaseViewModel
    {
        public EncryptionPackageExportViewModel(
            IServiceProvider serviceProvider) 
            : base(null, serviceProvider)
        {
        }
    }
}
