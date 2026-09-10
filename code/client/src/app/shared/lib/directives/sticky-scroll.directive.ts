import { Directive } from '@angular/core';

@Directive({
  selector: '[appStickyScroll]',
  standalone: true,
  host: {
    '[style.overflow-x]': "'auto'",
    '[style.overscroll-behavior-inline]': "'contain'",
  },
})
export class StickyScrollDirective {}
