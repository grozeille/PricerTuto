Feature: BlackSholes princing
	In order trade
	As a trader
	I want to price options

Scenario: Be able to price an european Call
	Given the following option:
		| type | spot   | strike | maturity | rate | volatility |
		| Call | 564.51 | 565    | 6 months | 0.01 | 0.225      |
	When I compute the price
	Then the result should be 43.95

Scenario: Be able to price an european Put
	Given the following option:
		| type | spot   | strike | maturity | rate | volatility |
		| Put  | 564.51 | 565    | 6 months | 0.01 | 0.225      |
	When I compute the price
	Then the result should be 40.49