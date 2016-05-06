Single
======

Single is an easy-to-use single-molecule imaging program!

Single allows to control unlimited number of shutters,
also supports N-color alternate excitation(ALEX), automatic focusing, and automatic flow features.

Single is written in C#, and completely free to use.

Single is currently being used by
 - Single Molecule Biophysics Laboratory, Seoul National University
 - Center for Theragnosis, Korea Institute of Science and Technology
 - Nano-bio Photonics Division, Advanced Phothonics Research Institute (of Gwangju Institute of Science and Technology)
 - Nanobio Imaging Laboratory, Gwangju Institute of Science and Technology
 - Molecular Genome Engineering Laboratory, Hanyang University

Compatible Hardwares
---

1. Basic Imaging / ALEX
  1. Any EMCCD from Andor.
  2. Any ProEM EMCCDs from Princeton Instruments.
  3. Counterboards from National Instruments(NI). Currently the program has been only tested with NI PCI-6601/2 (http://sine.ni.com/nips/cds/view/p/lang/en/nid/211875), however, any NI counterboards should also work. For your information, PCI-6601 supports only 3 counters so you will need PCI-6602 (or above) if you want 4-color ALEX.
  4. Any electronic shutters compatible with the counterboards. For ALEX FRET imaging, we have used VS35 series from Uniblitz (VS35S2T1-24).

3. Automatic Focusing
  1. Z-axis stages. Currently, MS-2000 control unit from Applied Scientific Instrumentation, C-Focus from Mad City Labs, and E-516 control unit from Physik Instrumente are supported.
     
     For more information, read this article (please cite if you use this): [Hwang, W. et al. Autofocusing system based on optical astigmatism analysis of single-molecule images, *Opt. Express* **2012**, *20* (28), 29353-29360.](http://dx.doi.org/10.1364/OE.20.029353)

4. Automatic Pump Control
  1. Any Chemyx Fusion series syringe pumps.
  2. Harvard PHD 2000 syringe pump.

Prerequisites
---
1. Auto-focusing and Auto flow runs in seperate threads, so *CPU with quad core or more cores is recommended*. Also vertical screen resolution should be more than 1050 pixels in order to display settings panel properly.
2. NI DAQmx. Download it here: http://www.ni.com/download/ni-daqmx-14.1/4953/en/
3. All other required device drivers (EMCCD, counterboards, Z-axis stages, etc.). Note that DLL for Andor CCD (atmcd32cs.dll and atmcd32d.dll), PI GCS2 piezo stage (E7XX_GCS2_DLL.dll), and for Mad City Labs' C-Focus (Madlib.dll) is already included with the distribution; you can exchange one of the dlls to the one working with your setup.
4. [DotNet framework 4.5](https://www.microsoft.com/download/details.aspx?id=30653) (or above). If you have Visual Studio 2013 or above, then you can skip it. DotNet framework is also installable via Setup.exe.

Installation
---
1. Download Binary distribition here: https://github.com/pjb7687/single/releases/download/v1.0.4/Singlev1.0.4-Setup.exe
   or Build your own binary from source code.
2. Download and install all prerequisites. See above section.
3. Run Single.
4. In *Settings* tab, set all values according to your setup.

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
