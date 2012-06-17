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

Scenario Outline: Be able to price an european Option
	Given the following option:
		| type   | spot   | strike   | maturity   | rate   | volatility   |
		| <type> | <spot> | <strike> | <maturity> | <rate> | <volatility> |
	When I compute the price
	Then the result should be <price>

	Examples:
		| type | spot   | strike | maturity | rate | volatility | price |
		| Put  | 564.51 | 565    | 6 months | 0.01 | 0.225      | 43.95 | 
		| Call | 564.51 | 565    | 6 months | 0.01 | 0.225      | 43.95 |
		| Call | 564.51 | 565    | 1 year   | 0.01 | 0.225      | 63.32 |

Scenario: Be able to price an european Call with market data
	Given the market rate: 0.01
	And the market spot for underlying "NASDAQ:GOOG": 564.51
	And the market volatility for underlying "NASDAQ:GOOG" since 6 months: 0.225
	When I compute the price with market data the option:
		| type | strike | maturity | underlying  |
		| Call | 565    | 6 months | NASDAQ:GOOG |
	Then the result should be 43.95