#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <string.h>
#include <stdint.h>
#include <avr/sleep.h>
#include <avr/wdt.h>

#include "rfm12.h"

//#define LED_DEBUG

#define LED_PORT  	PORTB
#define LED_DDR    	DDRB
#define LED_BIT     PB0

#define VD_OFF  	LED_PORT |= 1 << LED_BIT;
#define VD_ON  		LED_PORT &= ~(1 << LED_BIT);

#define CHIP_ID 	1


void _blink(void)
{
#if defined(LED_DEBUG)
	VD_ON;
	_delay_ms(50);
	VD_OFF;
	//_delay_ms(100);
#endif
}

// ADC interrupt service routine
ISR(ADC_vect)
{
	if (ADCH > 50)
	{
		uint8_t packet[] = {CHIP_ID, ADCL, ADCH}; //order in which bytes are read is important
		uint8_t packettype = 0xEE;
		
		rfm12_tx (sizeof(packet), packettype, packet);
		rfm12_tick();

		_blink();			
	}
}


int main(void)
{             
#ifdef LED_DEBUG     
	LED_DDR |= _BV(LED_BIT); //enable LED if any
#endif

	_delay_ms(100); //little delay for the rfm12 to initialize properly
	rfm12_init();   //init the RFM12
	_delay_ms(250); //little delay for the rfm12 to initialize properly

	
	ADCSRA |= (1 << ADPS2) | (1 << ADPS1) | (1 << ADPS0); // Set ADC prescaler to 128 - 125KHz sample rate @ 16MHz

	ADMUX |= (1 << REFS0); // Set ADC reference to AVCC
	ADMUX |= (1 << ADLAR); // Left adjust ADC result to allow easy 8 bit reading

	// No MUX values needed to be changed to use ADC0

	ADCSRA |= (1 << ADFR);  // Set ADC to Free-Running Mode
	ADCSRA |= (1 << ADEN);  // Enable ADC

	ADCSRA |= (1 << ADIE);  // Enable ADC Interrupt

	// Disable unused modules
	ACSR |= (1 << ACD);     // Disable Analog Comparator
	wdt_disable();			 // Disable Watchdog Timer
	
	
	sei();          //interrupts on
	
	_blink();		//ready
	
	set_sleep_mode(SLEEP_MODE_IDLE);
	sleep_enable();
	
	ADCSRA |= (1 << ADSC);  // Start A2D Conversions
	
	while (1)
	{
		sleep_mode();
	};
}
