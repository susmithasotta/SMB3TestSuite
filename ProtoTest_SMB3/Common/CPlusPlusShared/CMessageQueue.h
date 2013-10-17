#ifndef _MESSAGEQUEUE
#define _MESSAGEQUEUE

#pragma once

#include <windows.h>
#include <queue>

using namespace std;

const unsigned int DEFAULT_TIMEOUT_INTERVAL = INFINITE;
const unsigned int DEFAULT_MAX_QUEUE_SIZE = 1024;

//---------------------
// Message Queue Class
//---------------------
template <class T>
class CMessageQueue
{
private:
	// message queue
	queue<T> m_qMsgQueue;
	
	// queue mutex
	HANDLE m_hQueueMutex; 
	
	// queue writable event
	HANDLE m_hWritableEvent;

	// queue readable event
	HANDLE m_hReadableEvent;

	// time out interval (of queue push/pop transaction)
	unsigned int m_nTimeoutMilliseconds;

	// maximum allowed queue size
	unsigned int m_nMaxSize;

public:
	
	// Constructor: with parameters
	CMessageQueue(unsigned int nMaxSize = DEFAULT_MAX_QUEUE_SIZE, unsigned int nTimeoutMilliseconds = DEFAULT_TIMEOUT_INTERVAL);
	
	// Destructor
	virtual ~CMessageQueue();

public:
	// Enqueue Message
	bool Put(T msg);

	// Dequeue Message
	bool Get(T& msg);

public:
	// Get queue size
	unsigned int GetSize();

	// Set Timeout Interval
	void SetTimeout(unsigned int nTimeoutMilliseconds);
};

//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::CMessageQueue
//
// Usage:
//	- Class Constructor
//
// Prameters:
//	- unsigned int nMaxSize:	the maximum allowed queue size
//	- unsigned int nTimeoutMilliseconds:	time out interval of queue transaction
//
// Returns:
//	- N/A
//----------------------------------------------------------------------------------
template <class T>
CMessageQueue<T>::CMessageQueue(unsigned int nMaxSize, unsigned int nTimeoutMilliseconds)
{
    // initialize event handles
	m_hWritableEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
	m_hReadableEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

	// initialize mutex handle
	m_hQueueMutex = CreateMutex(NULL, FALSE, NULL);

	// set timeout interval (by milliseconds)
	m_nTimeoutMilliseconds = nTimeoutMilliseconds;

	// set maximum allowed queue size
	m_nMaxSize = nMaxSize;

	// enable queue writability according to nMaxSize
	if (m_nMaxSize > 0)
	{
		SetEvent(m_hWritableEvent);
	}
}


//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::~CMessageQueue
//
// Usage:
//	- Class Destructor
//
// Prameters:
//  - N/A
//
// Returns:
//	- N/A
//----------------------------------------------------------------------------------
template <class T>
CMessageQueue<T>::~CMessageQueue()
{
	// hold mutex
	WaitForSingleObject(m_hQueueMutex, INFINITE);
	
	// clear queue
	m_qMsgQueue.empty();	

	// close event handles
	CloseHandle(m_hWritableEvent);
	CloseHandle(m_hReadableEvent);
	
	// release mutex
	ReleaseMutex(m_hQueueMutex);

	// close mutex handle
	CloseHandle(m_hQueueMutex);	
}


//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::Put
//
// Usage:
//	- To put a message into queue
//
// Prameters:
//	- T msg:	the message to be put in queue
//
// Returns:
//	- bool: function success/fail status
//----------------------------------------------------------------------------------
template <class T>
bool CMessageQueue<T>::Put(T msg)
{
	// hold mutex
	WaitForSingleObject(m_hQueueMutex, INFINITE);

	// check writablity
	while(m_nMaxSize == m_qMsgQueue.size())
	{
		if( WAIT_OBJECT_0 != SignalObjectAndWait(m_hQueueMutex, m_hWritableEvent, m_nTimeoutMilliseconds, false))
		{	
			return false;
		}
		WaitForSingleObject(m_hQueueMutex, INFINITE);
	}

	// put message to queue
	m_qMsgQueue.push(msg);

	// disable writable if queue is full
	if (m_nMaxSize == m_qMsgQueue.size())
	{
		ResetEvent(m_hWritableEvent);
	}

	// enable readable if queue is no longer empty
	if(1 == m_qMsgQueue.size())
	{
		SetEvent(m_hReadableEvent);
	}

	// release mutex
	ReleaseMutex(m_hQueueMutex);

	// put succeeded
	return true;
}

//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::Get
//
// Usage:
//	- To get a message from queue
//
// Prameters:
//  - T& msg:	the message reference to get from queue
//
// Returns:
//	- bool: function success/fail status
//----------------------------------------------------------------------------------
template <class T>
bool CMessageQueue<T>::Get(T& msg)
{
	// hold mutex
	WaitForSingleObject(m_hQueueMutex, INFINITE);

	// check readability
	while (m_qMsgQueue.size() < 1)
	{
		if( WAIT_OBJECT_0 != SignalObjectAndWait(m_hQueueMutex, m_hReadableEvent, m_nTimeoutMilliseconds, false))
		{	
			return false;
		}
		WaitForSingleObject(m_hQueueMutex, INFINITE);
	}

	// get first item in queue
	msg = m_qMsgQueue.front();
	m_qMsgQueue.pop();

	// disable readable if queue is empty
	if (0 == m_qMsgQueue.size())
	{
		ResetEvent(m_hReadableEvent);
	}

	// enable writable if queue is no longer full
	if (m_nMaxSize - 1 == m_qMsgQueue.size())
	{
		SetEvent(m_hWritableEvent);
	}

	// release mutex
	ReleaseMutex(m_hQueueMutex);

	// popped message
	return true;
}

//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::GetSize
//
// Usage:
//	- To get the current queue size
//
// Prameters:
//	- N/A
//
// Returns:
//	- unsigned int: the current queue size
//----------------------------------------------------------------------------------
template <class T>
unsigned int CMessageQueue<T>::GetSize()
{
	// hold mutex
	WaitForSingleObject(m_hQueueMutex, INFINITE);

	// get size
	int nCurrentSize = m_qMsgQueue.size();
	
	// release mutex
	ReleaseMutex(m_hQueueMutex);

	// return size
	return nCurrentSize;
}

//---------------------------------------------------------------------------------
// Function Name: 
//	- CMessageQueue<T>::SetTimeout
//
// Usage:
//	- To set time out interval
//
// Prameters:
//	- unsigned int nTimeoutMilliseconds:	time out interval in milliseconds
//
// Returns:
//	- N/A
//----------------------------------------------------------------------------------
template <class T>
void CMessageQueue<T>::SetTimeout(unsigned int nTimeoutMilliseconds)
{
	// hold mutex
	WaitForSingleObject(m_hQueueMutex, INFINITE);

	// set timeout interval (by milliseconds)
	m_nTimeoutMilliseconds = nTimeoutMilliseconds;
	
	// release mutex
	ReleaseMutex(m_hQueueMutex);

	return;
}
#endif /*_MESSAGEQUEUE*/