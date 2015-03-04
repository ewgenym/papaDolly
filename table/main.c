#include <avr/io.h>
#include <avr/interrupt.h>
#include <avr/pgmspace.h>
#include <util/delay.h>
#include <string.h>
#include <stdint.h>
#include <avr/sleep.h>
#include <avr/wdt.h>

#include "rfm12.h"

#define LED_PORT  	PORTB
#define LED_DDR    	DDRB
#define LED_BIT     PB0

#define VD_OFF  	LED_PORT |= 1 << LED_BIT;
#define VD_ON  		LED_PORT &= ~(1 << LED_BIT);

#define CHIP_ID 	1


void _blink(void)
{
	VD_ON;
	_delay_ms(10);
	VD_OFF;
}

ISR (INT0_vect)
{
	uint8_t packet[] = {CHIP_ID};
	uint8_t packettype = 0xEE;
		
	rfm12_tx (sizeof(packet), packettype, packet);
	rfm12_tick();
	_delay_ms(1);

    //_blink();
}


int main(void)
{             
	LED_DDR |= _BV(LED_BIT); //enable LED if any
	VD_OFF;

	_delay_ms(100); //little delay for the rfm12 to initialize properly
	rfm12_init();   //init the RFM12
	_delay_ms(250); //little delay for the rfm12 to initialize properly

	// Disable unused modules
	ADCSRA &= ~(1<<ADEN); 	 //Disable ADC
	ACSR |= (1 << ACD);     // Disable Analog Comparator
	wdt_disable();			 // Disable Watchdog Timer
	
    DDRD &= ~(1 << DDD2);     // Clear the PD2 pin
    // PD2 (INT0 pin) is now an input

    PORTD |= (1 << PORTD2);    // turn On the Pull-up
    // PD0 is now an input with pull-up enabled

    MCUCR |= (1 << ISC00);    // set INT0 to trigger on ANY logic change
    GICR |= (1 << INT0);      // Turns on INT0
	
	sei();                    // turn on interrupts
	
	_blink();
	
	set_sleep_mode(SLEEP_MODE_IDLE);
	sleep_enable();

    while(1)
    {
        sleep_mode();
    }
}

