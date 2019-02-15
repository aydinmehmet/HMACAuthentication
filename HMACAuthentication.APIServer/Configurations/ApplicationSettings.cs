namespace HMACAuthentication.APIServer.Configurations
{
    /// <summary>
    /// The ApplicationSettings
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets or sets the logging.
        /// </summary>
        /// <value>
        /// The logging.
        /// </value>
        public Logging Logging { get; set; }
        /// <summary>
        /// Gets or sets the hmac configurations.
        /// </summary>
        /// <value>
        /// The hmac configurations.
        /// </value>
        public HMACConfigurations HMACConfigurations { get; set; }
    }
    /// <summary>
    /// The LogLevel
    /// </summary>
    public class LogLevel
    {
        /// <summary>
        /// Gets or sets the default.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public string Default { get; set; }
        /// <summary>
        /// Gets or sets the system.
        /// </summary>
        /// <value>
        /// The system.
        /// </value>
        public string System { get; set; }
        /// <summary>
        /// Gets or sets the microsoft.
        /// </summary>
        /// <value>
        /// The microsoft.
        /// </value>
        public string Microsoft { get; set; }
    }
    /// <summary>
    /// The Logging
    /// </summary>
    public class Logging
    {
        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        public LogLevel LogLevel { get; set; }
    }
    /// <summary>
    /// The Web
    /// </summary>
    public class Web
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string ID { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
    /// <summary>
    /// The Android
    /// </summary>
    public class Android
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string ID { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
    /// <summary>
    /// The IPhone
    /// </summary>
    public class IPhone
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string ID { get; set; }
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }
    }
    /// <summary>
    /// The AllowedApplication
    /// </summary>
    public class AllowedApplication
    {
        /// <summary>
        /// Gets or sets the web.
        /// </summary>
        /// <value>
        /// The web.
        /// </value>
        public Web Web { get; set; }
        /// <summary>
        /// Gets or sets the android.
        /// </summary>
        /// <value>
        /// The android.
        /// </value>
        public Android Android { get; set; }
        /// <summary>
        /// Gets or sets the i phone.
        /// </summary>
        /// <value>
        /// The i phone.
        /// </value>
        public IPhone IPhone { get; set; }
    }
    /// <summary>
    /// The HMACConfigurations
    /// </summary>
    public class HMACConfigurations
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is disable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disable; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisable { get; set; }
        /// <summary>
        /// Gets or sets the allowed application.
        /// </summary>
        /// <value>
        /// The allowed application.
        /// </value>
        public AllowedApplication AllowedApplication { get; set; }
    }
}
