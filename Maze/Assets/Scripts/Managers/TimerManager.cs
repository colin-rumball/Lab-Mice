using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Game))]

public class TimerManager : MonoBehaviour 
{
	// Stores the timers after creation for easy management.
	private List<Timer> timers = new List<Timer>();

	//--------------------------------------------------------------
	/// Called automatically once per frame
	//--------------------------------------------------------------
	public void Update () 
	{
		foreach(Timer timer in timers)
		{
			if (timer.isRunning())
			{
				if (timer.getTimeLeft() <= 0.0f)
					timer.setToExpired();
				else
					timer.decreaseTimer();
			}
		}
	}

	//--------------------------------------------------------------
	/// Use to start a timer of passed name and length from the start.
	//--------------------------------------------------------------
	public void startTimer(string _name, float _length)
	{
		foreach(Timer timer in timers)
		{
			if (timer.getName() == _name)
			{
				timer.reset();
				timer.start();
				return;
			}
		}
		Timer t = new Timer(_name, _length);
		timers.Add(t);
	}

	//--------------------------------------------------------------
	/// Returns a timer of passed name. (Can return Null)
	//--------------------------------------------------------------
	public Timer getTimer(string _name)
	{
		foreach(Timer timer in timers)
		{
			if (timer.getName() == _name)
			{
				return timer;
			}
		}
		return null;
	}

	//--------------------------------------------------------------
	/// Use to remove a timer of passed name.
	//--------------------------------------------------------------
	public void removeTimer(string _name)
	{
		foreach(Timer timer in timers)
		{
			if (timer.getName() == _name)
			{
				timers.Remove(timer);
				return;
			}
		}
	}

	//--------------------------------------------------------------
	/// Use to pause all running timers.
	//--------------------------------------------------------------
	public void pauseAllTimers()
	{
		foreach(Timer timer in timers)
		{
			if (timer.isRunning())
				timer.pause();
		}
	}

	//--------------------------------------------------------------
	/// Use to unpause all paused timers.
	//--------------------------------------------------------------
	public void unPauseAllTimers()
	{
		foreach(Timer timer in timers)
		{
			if (timer.isPaused())
				timer.unPause();
		}
	}
}
