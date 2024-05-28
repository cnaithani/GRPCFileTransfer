New Implementation
- Separate Client and Agent Services
- Dependency Injection
- Logging 
- Config Entries
- Instance Management - Singleton, Scoped, Transient
- Disconnected Flow
- Ability to transfer in different environment (MAC)

Remaining
- Error Handling
	- Fault Contract
	- Unhandled Errors
- Anonymous Access
- Handling Timeouts
- MAC Deployment


-------------------------------------------
Limitations - 
1. No state management in GRPC, Need to be handled in application - Found Workaround
2. Only one GRPC Proto can be implemented in an service 
3. Speed Noted - 220MB transferred from windows to MAC in 2 minutes at speed of 10-13 Megabits per second
 