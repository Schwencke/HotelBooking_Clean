Feature: BookingValidation

Validation feature which is used to validate that fully occupied dates are calculated correctly


Scenario Template: Booking within a occupied period
	Given the start date is <startDays> from now
	And the end date is <endDays> from now
	And the id of the room is <roomId>
	When the booking is created
	Then the endpoint should return <bookingResult>

	Examples: 
	| startDays | endDays | roomId | bookingResult |
	| 20        | 24      |    1   | true          |
	| 20        | 24      |    2   | true          |
	| 1         | 9       |    1   | true          |
	| 1         | 9       |    2   | true          |
	| 10        | 11      |    2   | false         |
	| 10        | 11      |    1   | false         |
	| 10        | 18      |    2   | false         |
	| 10        | 18      |    1   | false         |
	| 9         | 20      |    2   | false         |
	| 9         | 20      |    1   | false         |
	| 11        | 19      |    2   | false         |
	| 11        | 19      |    1   | false         |
	| 100       | 101	  |    2   | true          |
	| 100       | 101     |    1   | true          |
