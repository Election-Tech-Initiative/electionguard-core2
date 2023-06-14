using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Converters;

namespace ElectionGuard.UI.Converters
{
    public abstract class TallyStateConverter : BaseConverter<TallyState, bool>
    {
        public TallyStateConverter(TallyState state)
        {
            _state = state;
        }

        protected readonly TallyState _state;

        public override bool DefaultConvertReturnValue { get; set; }

        public override TallyState DefaultConvertBackReturnValue { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override TallyState ConvertBackTo(bool value, CultureInfo? culture) => throw new NotImplementedException();
        public override bool ConvertFrom(TallyState value, CultureInfo? culture) => value < _state;
    }
}
