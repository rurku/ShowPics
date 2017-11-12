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
        void Option(string template, string description, CommandOptionType optionType, Action<CommandOption> callback, Action<CommandOption> configuration = null);
    }
}
