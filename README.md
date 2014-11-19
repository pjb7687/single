Single
======

Single is an easy-to-use single-molecule imaging program!

Single allows to control unlimited number of shutters,
also supports N-color alternate excitation(ALEX), automatic focusing, and automatic flow features.

Single is written in C#, and completely free to use.

Compatible Hardwares
---

1. Basic Imaging / ALEX
  1. Any EMCCD from Andor.
  2. Any ProEM EMCCDs from Princeton Instruments.
  3. Counterboards from National Instruments(NI). Currently the program has been tested with NI PCI-6601/2 (http://sine.ni.com/nips/cds/view/p/lang/ko/nid/211875), however, The program uses NI standard interface (a.k.a. VISA, Virtual Instrument Software Architecture) to communicate with counterboards, so any VISA-compatible counterboards will work. For your information, PCI-6601 supports only 3 counters so you will need PCI-6602 (or above) if you want 4-color ALEX.
  4. Any electronic shutters compatible with the counterboards. For ALEX FRET imaging, we have used VS35 series from Uniblitz (VS35S2T1-24).

3. Automatic Focusing
  1. Z-axis stages. Currently, MS-2000 control unit from Applied Scientific Instrumentation, C-Focus from Mad City Labs, and E-516 control unit from Physik Instrumente are supported. For more information, read this article: http://dx.doi.org/10.1364/OE.20.029353

4. Automatic Pump Control
  1. Any Chemyx Fusion series syringe pumps.
  2. Harvard PHD 2000 syringe pump.

Prerequisites
---
1. All required device drivers (EMCCD, counterboards, Z-axis stages, etc.).
2. NI VISA with Visual Studio Examples. To install it properly, you should *customize* your setup and ensure to install *Development Support*-*.Net Framework xxx Lauguages Support*-*Examples*. It installs not only examples but also required .Net DLLs.
3. NI DAQmx with Visual Studio Examples. To install it properly, you should *customize* your setup and ensure to install *Development Support*-*.Net Framework xxx Lauguages Support*-*Examples*. It installs not only examples but also required .Net DLLs.
4. DotNet framework. If you have Visual Studio, then you can skip it.

License
---

Copyright (c) 2013, Jeongbin Park
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list
of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or other
materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
