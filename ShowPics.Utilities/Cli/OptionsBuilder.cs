using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Utilities.Cli
{
    public class CliOptionsBuilder : ICliOptionsBuilder
    {
        private CommandLineApplication _app;
        private List<Action> _callbacks = new List<Action>();

        public CliOptionsBuilder(CommandLineApplication app)
        {
            _app = app;
        }

        public void Option(string template, string description, CommandOptionType optionType, Action<CommandOption> callback, Action<CommandOption> configuration = null)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            CommandOption option;
            if (configuration != null)
                option = _app.Option(template, description, optionType, configuration);
            else
                option = _app.Option(template, description, optionType);
            _callbacks.Add(() => callback(option));
        }

        public void ExecuteCallbacks()
        {
            foreach (var callback in _callbacks)
                callback();
        }
    }

    public interface ICliOptionsBuilder
    {
        /// <summary>
        /// Configure a command line option
        /// </summary>
        /// <param name="template">Option template</param>
        /// <param name="description">Description to be shown in help</param>
        /// <param name="optionType">Option type</param>
        /// <param name="callback">
        ///     Callback delegate which will be executed after parsing the commandline. 
        ///     <para>Use it to read the option value. The callback is executed even if the option is not used.</para>
        /// </param>
        /// <param name="configuration">Additional option configuration action</param>
        void Option(string template, string description, CommandOptionType optionType, Action<CommandOption> callback, Action<CommandOption> configuration = null);
    }
}
