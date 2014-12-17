using UnityEngine;
using System.Collections;

public class Timer
{
	private string Name;
	private float TimerLength;
	private float TimeLeft;
	private bool Running;
	private bool Paused;
	private bool Expired;

	public Timer(string _name, float _length)
	{
		Name = _name;
		TimerLength = _length;
		TimeLeft = _length;
		this.start();
	}

	public void decreaseTimer()
	{
		TimeLeft -= Time.deltaTime;
	}

	public void pause()
	{
		Paused = true;
		Running = false;
	}

	public void unPause()
	{
		Paused = false;
		Running = true;
	}

	public void start()
	{
		Running = true;
		Expired = false;
	}

	public void reset()
	{
		TimeLeft = TimerLength;
	}

	public bool isRunning()
	{
		return Running;
	}

	public bool isPaused()
	{
		return Paused;
	}

	public bool hasExpired()
	{
		return Expired;
	}

	public void setToExpired()
	{
		Expired = true;
		Running = false;
	}

	public float getLength()
	{
		return TimerLength;
	}

	public float getTimeLeft()
	{
		return TimeLeft;
	}

	public string getName()
	{
		return Name;
	}
}
