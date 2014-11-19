Single
======

Single is an easy-to-use single-molecule imaging program!

Single allows to control unlimited number of shutters,
also supports N-color alternate excitation(ALEX), automatic focusing, and automatic flow features.

Single is written in C#, and completely free to use.

License
------

Single is distributed under the BSD license.
See LICENSE for more information!

Compatible Hardwares
---

1. Basic Imaging
  1. Any EMCCD from Andor.
  2. Any ProEM EMCCDs from Princeton Instruments.

2. ALEX
  1. Counterboards from National Instruments(NI). Currently the program has been tested with NI PCI-6601/2 (http://sine.ni.com/nips/cds/view/p/lang/ko/nid/211875), however, The program uses NI standard interface (a.k.a. VISA, Virtual Instrument Software Architecture) to communicate with counterboards, so any VISA-compatible counterboards will work. For your information, PCI-6601 supports only 3 counters so you will need PCI-6602 (or above) if you want 4-color ALEX.
  2. Any electronic shutters compatible with the counterboards. For ALEX FRET imaging, we have used VS35 series from Uniblitz (VS35S2T1-24).

3. Automatic Focusing
  1. Z-axis stages. Currently, MS-2000 control unit from Applied Scientific Instrumentation, C-Focus from Mad City Labs, and E-516 control unit from Physik Instrumente are supported. For more information, read this article: http://dx.doi.org/10.1364/OE.20.029353

4. Automatic Pump Control
  1. Any Chemyx Fusion series syringe pumps.
  2. Harvard PHD 2000 syringe pump.

