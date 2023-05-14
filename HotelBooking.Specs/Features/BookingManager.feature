Feature: BookingManager

A customer can create a booking for a specific date and time.
If a room is avalible within the whole time period, the booking is created.

Scenario: start date after, and > end date, end date after
	Given start date after, and > end date
	And end date after occupied time
	When creating a booking
	Then the booking should not be created
	
Scenario: Start date before, end date before
	Given start date before occupied time
	And end date before occupied time
	When creating a booking
	Then the booking should be created

Scenario: Start date after, end date after
	Given start date after occupied time
	And end date after occupied time
	When creating a booking
	Then the booking should be created

Scenario: Start date before, end date after
	Given start date before occupied time
	And end date after occupied time
	When creating a booking
	Then the booking should not be created

Scenario: Start date in occupied, end date after occupied
	Given start date in occupied time
	And end date after occupied time
	When creating a booking
	Then the booking should not be created

Scenario: start date before, end date in occupied
	Given start date before occupied time
	And end date in occupied time
	When creating a booking
	Then the booking should not be created