using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ladder_diagram_app.Models.Variables.Instances
{
    /// <summary>
    /// Abstract base class for variables in a ladder diagram, providing common properties and change notification.
    /// </summary>
    public abstract class Variable : INotifyPropertyChanged
    {
        /// <summary>
        /// Stores the name of the variable.
        /// </summary>
        private string _name;

        /// <summary>
        /// Stores the type of the variable.
        /// </summary>
        private string _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable"/> class with default empty values.
        /// </summary>
        protected Variable()
        {
            _name = string.Empty;
            _type = string.Empty;
        }

        /// <summary>
        /// Gets or sets the name of the variable, notifying subscribers on change.
        /// </summary>
        public string Name
        {
            get => _name; 
            set => SetField(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the type of the variable, notifying subscribers on change.
        /// </summary>
        public string Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the variable can be deleted.
        /// </summary>
        public bool IsDeletable { get; set; } = true;

        /// <summary>
        /// Event raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed, automatically inferred if not specified.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Updates a field and raises the <see cref="PropertyChanged"/> event if the value changes.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="field">The backing field to update.</param>
        /// <param name="value">The new value for the field.</param>
        /// <param name="propertyName">The name of the property, automatically inferred if not specified.</param>
        /// <returns>True if the field was updated, false if the value was unchanged.</returns>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Converts the variable's properties to a dictionary for export purposes.
        /// </summary>
        /// <returns>A dictionary containing the variable's properties and their values.</returns>
        public abstract Dictionary<string, object> ToExportDictionary();
    }
}
