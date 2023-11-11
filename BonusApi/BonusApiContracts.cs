//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#nullable enable

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 612 // Disable "CS0612 '...' is obsolete"
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."
#pragma warning disable 8073 // Disable "CS8073 The result of the expression is always 'false' since a value of type 'T' is never equal to 'null' of type 'T?'"
#pragma warning disable 3016 // Disable "CS3016 Arrays as attribute arguments is not CLS-compliant"
#pragma warning disable 8603 // Disable "CS8603 Possible null reference return"
#pragma warning disable 8604 // Disable "CS8604 Possible null reference argument for parameter"

namespace BonusApi
{
    using System = global::System;

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial interface IBonusClient
    {
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task ApiAccrualManualAsync(AccrualManualDto? body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task ApiAccrualManualAsync(AccrualManualDto? body, System.Threading.CancellationToken cancellationToken);

        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiBalanceAsync(System.Guid personId, int? bankId);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiBalanceAsync(System.Guid personId, int? bankId, System.Threading.CancellationToken cancellationToken);

        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<BonusProgram>> ApiBonusProgramAsync();

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<System.Collections.Generic.ICollection<BonusProgram>> ApiBonusProgramAsync(System.Threading.CancellationToken cancellationToken);

        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiPayAsync(PayDto? body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiPayAsync(PayDto? body, System.Threading.CancellationToken cancellationToken);

        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiPayManualAsync(PayManualDto? body);

        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Success</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        System.Threading.Tasks.Task<long> ApiPayManualAsync(PayManualDto? body, System.Threading.CancellationToken cancellationToken);

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class AccrualManualDto : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Guid _personId = default!;
        private int? _bankId = default!;
        private int? _sum = default!;
        private string _description = default!;
        private string _transactionId = default!;
        private System.Guid _userId = default!;

        [System.Text.Json.Serialization.JsonPropertyName("personId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid PersonId
        {
            get { return _personId; }

            set
            {
                if (_personId != value)
                {
                    _personId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bankId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? BankId
        {
            get { return _bankId; }

            set
            {
                if (_bankId != value)
                {
                    _bankId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("sum")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? Sum
        {
            get { return _sum; }

            set
            {
                if (_sum != value)
                {
                    _sum = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("description")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("transactionId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string TransactionId
        {
            get { return _transactionId; }

            set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid UserId
        {
            get { return _userId; }

            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static AccrualManualDto FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<AccrualManualDto>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class BonusProgram : System.ComponentModel.INotifyPropertyChanged
    {
        private int? _id = default!;
        private string? _name = default!;
        private ProgramTypes? _programTypes = default!;
        private string? _description = default!;
        private System.DateTimeOffset? _activeFrom = default!;
        private System.DateTimeOffset? _activeTo = default!;
        private System.Collections.Generic.ICollection<BonusProgramLevel>? _programLevels = default!;
        private bool? _isDeleted = default!;
        private System.DateTimeOffset? _lastUpdated = default!;
        private System.Collections.Generic.ICollection<int>? _bankId = default!;

        [System.Text.Json.Serialization.JsonPropertyName("id")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public int? Id
        {
            get { return _id; }

            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("name")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public string? Name
        {
            get { return _name; }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("programTypes")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
        public ProgramTypes? ProgramTypes
        {
            get { return _programTypes; }

            set
            {
                if (_programTypes != value)
                {
                    _programTypes = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("description")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public string? Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("activeFrom")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? ActiveFrom
        {
            get { return _activeFrom; }

            set
            {
                if (_activeFrom != value)
                {
                    _activeFrom = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("activeTo")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? ActiveTo
        {
            get { return _activeTo; }

            set
            {
                if (_activeTo != value)
                {
                    _activeTo = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("programLevels")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public System.Collections.Generic.ICollection<BonusProgramLevel>? ProgramLevels
        {
            get { return _programLevels; }

            set
            {
                if (_programLevels != value)
                {
                    _programLevels = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("isDeleted")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public bool? IsDeleted
        {
            get { return _isDeleted; }

            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("lastUpdated")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public System.DateTimeOffset? LastUpdated
        {
            get { return _lastUpdated; }

            set
            {
                if (_lastUpdated != value)
                {
                    _lastUpdated = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bankId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public System.Collections.Generic.ICollection<int>? BankId
        {
            get { return _bankId; }

            set
            {
                if (_bankId != value)
                {
                    _bankId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static BonusProgram FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<BonusProgram>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class BonusProgramLevel : System.ComponentModel.INotifyPropertyChanged
    {
        private int? _id = default!;
        private string? _name = default!;
        private System.DateTimeOffset? _lastUpdated = default!;
        private int? _level = default!;
        private int? _programId = default!;
        private BonusProgram? _bonusProgram = default!;
        private long? _condition = default!;
        private int? _benefit = default!;
        private string? _description = default!;

        [System.Text.Json.Serialization.JsonPropertyName("id")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public int? Id
        {
            get { return _id; }

            set
            {
                if (_id != value)
                {
                    _id = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("name")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public string? Name
        {
            get { return _name; }

            set
            {
                if (_name != value)
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("lastUpdated")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public System.DateTimeOffset? LastUpdated
        {
            get { return _lastUpdated; }

            set
            {
                if (_lastUpdated != value)
                {
                    _lastUpdated = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("level")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public int? Level
        {
            get { return _level; }

            set
            {
                if (_level != value)
                {
                    _level = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("programId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public int? ProgramId
        {
            get { return _programId; }

            set
            {
                if (_programId != value)
                {
                    _programId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bonusProgram")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public BonusProgram? BonusProgram
        {
            get { return _bonusProgram; }

            set
            {
                if (_bonusProgram != value)
                {
                    _bonusProgram = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("condition")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public long? Condition
        {
            get { return _condition; }

            set
            {
                if (_condition != value)
                {
                    _condition = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("benefit")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public int? Benefit
        {
            get { return _benefit; }

            set
            {
                if (_benefit != value)
                {
                    _benefit = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("description")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        public string? Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static BonusProgramLevel FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<BonusProgramLevel>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class GetBalanceDto : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Guid _personId = default!;
        private int? _bankId = default!;

        [System.Text.Json.Serialization.JsonPropertyName("personId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid PersonId
        {
            get { return _personId; }

            set
            {
                if (_personId != value)
                {
                    _personId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bankId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? BankId
        {
            get { return _bankId; }

            set
            {
                if (_bankId != value)
                {
                    _bankId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static GetBalanceDto FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<GetBalanceDto>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PayDto : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Guid _personId = default!;
        private int? _bankId = default!;
        private int? _sum = default!;
        private string _description = default!;
        private string _transactionId = default!;
        private System.Guid _ezsId = default!;

        [System.Text.Json.Serialization.JsonPropertyName("personId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid PersonId
        {
            get { return _personId; }

            set
            {
                if (_personId != value)
                {
                    _personId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bankId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? BankId
        {
            get { return _bankId; }

            set
            {
                if (_bankId != value)
                {
                    _bankId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("sum")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? Sum
        {
            get { return _sum; }

            set
            {
                if (_sum != value)
                {
                    _sum = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("description")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("transactionId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string TransactionId
        {
            get { return _transactionId; }

            set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("ezsId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid EzsId
        {
            get { return _ezsId; }

            set
            {
                if (_ezsId != value)
                {
                    _ezsId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static PayDto FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<PayDto>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class PayManualDto : System.ComponentModel.INotifyPropertyChanged
    {
        private System.Guid _personId = default!;
        private int? _bankId = default!;
        private int? _sum = default!;
        private string _description = default!;
        private string _transactionId = default!;
        private System.Guid _userId = default!;

        [System.Text.Json.Serialization.JsonPropertyName("personId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid PersonId
        {
            get { return _personId; }

            set
            {
                if (_personId != value)
                {
                    _personId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("bankId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? BankId
        {
            get { return _bankId; }

            set
            {
                if (_bankId != value)
                {
                    _bankId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("sum")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]   
        [System.ComponentModel.DataAnnotations.Range(1, int.MaxValue)]
        public int? Sum
        {
            get { return _sum; }

            set
            {
                if (_sum != value)
                {
                    _sum = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("description")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("transactionId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public string TransactionId
        {
            get { return _transactionId; }

            set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;
                    RaisePropertyChanged();
                }
            }
        }

        [System.Text.Json.Serialization.JsonPropertyName("userId")]

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]   
        [System.ComponentModel.DataAnnotations.Required]
        public System.Guid UserId
        {
            get { return _userId; }

            set
            {
                if (_userId != value)
                {
                    _userId = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ToJson()
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Serialize(this, options);

        }
        public static PayManualDto FromJson(string data)
        {

            var options = new System.Text.Json.JsonSerializerOptions();

            return System.Text.Json.JsonSerializer.Deserialize<PayManualDto>(data, options);

        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public enum ProgramTypes
    {

        [System.Runtime.Serialization.EnumMember(Value = @"PeriodicalMonthlySumByLevels")]
        PeriodicalMonthlySumByLevels = 0,

    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    internal class DateFormatConverter : System.Text.Json.Serialization.JsonConverter<System.DateTimeOffset>
    {
        public override System.DateTimeOffset Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            var dateTime = reader.GetString();
            if (dateTime == null)
            {
                throw new System.Text.Json.JsonException("Unexpected JsonTokenType.Null");
            }

            return System.DateTimeOffset.Parse(dateTime);
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, System.DateTimeOffset value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }



    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string? Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string? response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception? innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + ((response == null) ? "(null)" : response.Substring(0, response.Length >= 512 ? 512 : response.Length)), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "14.0.0.0 (NJsonSchema v11.0.0.0 (Newtonsoft.Json v13.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string? response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception? innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

#pragma warning restore  108
#pragma warning restore  114
#pragma warning restore  472
#pragma warning restore  612
#pragma warning restore 1573
#pragma warning restore 1591
#pragma warning restore 8073
#pragma warning restore 3016
#pragma warning restore 8603
#pragma warning restore 8604