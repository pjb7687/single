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
  1. Z-axis stages. Currently, MS-2000 control unit from Applied Scientific Instrumentation, C-Focus from Mad City Labs, and E-516 control unit from Physik Instrumente are supported.
     
     For more information, read this article: [Hwang, W. et al. Autofocusing system based on optical astigmatism analysis of single-molecule images, *Opt. Express* **2012**, *20* (28), 29353-29360.](http://dx.doi.org/10.1364/OE.20.029353)

4. Automatic Pump Control
  1. Any Chemyx Fusion series syringe pumps.
  2. Harvard PHD 2000 syringe pump.

Prerequisites
---
0. Auto-focusing and Auto flow runs in seperate threads, so CPU with quad core or more cores is desired. Also vertical screen resolution should be more than 1050 pixels.
1. All required device drivers (EMCCD, counterboards, Z-axis stages, etc.).
2. NI VISA with Visual Studio Examples. To install it properly, you should *customize* your setup and ensure to install *Development Support*-*.Net Framework xxx Lauguages Support*-*Examples*. It installs not only examples but also required .Net DLLs. (FYI, Visual Studio 2010 == .Net 4.0, Visual Studio 2013 == .Net 4.5.2). If you want to use binary distribution of Single, you should use the same version of NI VISA compiled with the binary. Download it here: http://www.ni.com/download/ni-visa-5.4/4230/en/
3. NI DAQmx with Visual Studio Examples. To install it properly, you should *customize* your setup and ensure to install *Development Support*-*.Net Framework xxx Lauguages Support*-*Examples*. It installs not only examples but also required .Net DLLs. If you want to use binary distribution of Single, you should use the same version of NI DAQmx compiled with the binary. Download it here: http://www.ni.com/download/ni-daqmx-14.1/4953/en/
4. DotNet framework. If you have Visual Studio, then you can skip it. If you want to use binary distribution of Single, you should use the same version of DotNet framework compiled with the binary. Download it here: http://www.microsoft.com/en-us/download/details.aspx?id=42643

Installation
---
1. Download Binary distribition here: https://github.com/pjb7687/single/releases/download/v1.0/Singlev1.0.zip
   or Build your own binary from source code.
2. Download and install all prerequisites. See above section.
3. Run Single2013.exe.
4. In *Settings* tab, change all values as your setup.

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
