using System;
using System.Collections.Generic;

namespace MeshServerLifecycle {
	public enum ProcessState {WithoutMesh, Searching, Downloading, HasMesh};

	public enum Command {FindServer, Download, DownloadFinished};

	public class Process
	{
		public class StateTransition
		{
			readonly ProcessState CurrentState;
			readonly Command Command;

			public StateTransition(ProcessState currentState, Command command)
			{
				CurrentState = currentState;
				Command = command;
			}

			public override int GetHashCode()
			{
				return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				StateTransition other = obj as StateTransition;
				return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
			}
		}

		Dictionary<StateTransition, ProcessState> transitions;
		public ProcessState CurrentState { get; private set; }

		public Process()
		{
			CurrentState = ProcessState.WithoutMesh;
			transitions = new Dictionary<StateTransition, ProcessState>
			{
				{ new StateTransition(ProcessState.WithoutMesh, Command.FindServer), ProcessState.Searching },
				{ new StateTransition(ProcessState.HasMesh, Command.FindServer), ProcessState.Searching },

				{ new StateTransition(ProcessState.Searching, Command.Download), ProcessState.Downloading },
				{ new StateTransition(ProcessState.Downloading, Command.DownloadFinished), ProcessState.HasMesh },
			};
		}

		public ProcessState GetNext(Command command)
		{
			StateTransition transition = new StateTransition(CurrentState, command);
			ProcessState nextState;
			if (!transitions.TryGetValue(transition, out nextState))
				throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
			return nextState;
		}

		public ProcessState MoveNext(Command command)
		{
			ProcessState lastState = CurrentState;
			CurrentState = GetNext(command);
			UnityEngine.Debug.Log (string.Format("Succesfully moved: {0} -({1})-> {2}", lastState, command, CurrentState));
			return CurrentState;
		}
	}

}