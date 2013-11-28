using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Single2013
{
    static class gaussFit
    {

        private const int NSIZE = 7; //If datasize is changed, the matrix inverse algorithm should be changed too.
        private const int DATASIZE = 49; //If datasize is changed, the matrix inverse algorithm should be changed too.
        private const int PARAMSIZE = 6;

        private static double MAX(double x, double y)
        {
            return ((x) > (y) ? (x) : (y));
        }
        private static double SIGN(double a, double b)
        {
            return ((b) >= 0.0f ? Math.Abs(a) : -Math.Abs(a));
        }


        private static double PYTHAG(double a, double b)
        {
            double at = Math.Abs(a), bt = Math.Abs(b), ct, result;

            if (at > bt) { ct = bt / at; result = at * Math.Sqrt(1.0f + ct * ct); }
            else if (bt > 0.0f) { ct = at / bt; result = bt * Math.Sqrt(1.0f + ct * ct); }
            else result = 0.0f;
            return (result);
        }

        private static void dsvd(double[] a, double[] w, double[] v)
        {
            int n = 6; int m = 6;
            int flag, i, its, j, jj, k, l=0, nm=0;
            double c, f, h, s, x, y, z;
            double anorm = 0.0f, g = 0.0f, scale = 0.0f;
            double[] rv1 = new double[6];

            /* Householder reduction to bidiagonal form */
            for (i = 0; i < n; i++)
            {
                /* left-hand reduction */
                l = i + 1;
                rv1[i] = scale * g;
                g = s = scale = 0.0f;
                if (i < m)
                {
                    for (k = i; k < m; k++)
                        if (a[k * n + i] < 0)
                        {
                            scale -= a[k * n + i];
                        }
                        else
                        {
                            scale += a[k * n + i];
                        }
                    if (scale != 0)
                    {
                        for (k = i; k < m; k++)
                        {
                            a[k * n + i] = a[k * n + i] / scale;
                            s += a[k * n + i] * a[k * n + i];
                        }
                        f = a[i * n + i];
                        g = -SIGN(Math.Sqrt(s), f);
                        h = f * g - s;
                        a[i * n + i] = (f - g);
                        if (i != n - 1)
                        {
                            for (j = l; j < n; j++)
                            {
                                for (s = 0.0f, k = i; k < m; k++)
                                    s += (a[k * n + i] * a[k * n + j]);
                                f = s / h;
                                for (k = i; k < m; k++)
                                    a[k * n + j] += (f * a[k * n + i]);
                            }
                        }
                        for (k = i; k < m; k++)
                            a[k * n + i] = (a[k * n + i] * scale);
                    }
                }
                w[i] = (scale * g);

                /* right-hand reduction */
                g = s = scale = 0.0f;
                if (i < m && i != n - 1)
                {
                    for (k = l; k < n; k++)
                        if (a[i * n + k] < 0)
                        {
                            scale -= a[i * n + k];
                        }
                        else
                        {
                            scale += a[i * n + k];
                        }

                    if (scale != 0)
                    {
                        for (k = l; k < n; k++)
                        {
                            a[i * n + k] = a[i * n + k] / scale;
                            s += a[i * n + k] * a[i * n + k];
                        }
                        f = a[i * n + l];
                        g = -SIGN(Math.Sqrt(s), f);
                        h = f * g - s;
                        a[i * n + l] = (f - g);
                        for (k = l; k < n; k++)
                            rv1[k] = a[i * n + k] / h;
                        if (i != m - 1)
                        {
                            for (j = l; j < m; j++)
                            {
                                for (s = 0.0f, k = l; k < n; k++)
                                    s += (a[j * n + k] * a[i * n + k]);
                                for (k = l; k < n; k++)
                                    a[j * n + k] += (s * rv1[k]);
                            }
                        }
                        for (k = l; k < n; k++)
                            a[i * n + k] = (a[i * n + k] * scale);
                    }
                }
                anorm = MAX(anorm, (Math.Abs(w[i]) + Math.Abs(rv1[i])));
            }

            /* accumulate the right-hand transformation */
            for (i = n - 1; i >= 0; i--)
            {
                if (i < n - 1)
                {
                    if (g!=0)
                    {
                        for (j = l; j < n; j++)
                            v[j * n + i] = ((a[i * n + j] / a[i * n + l]) / g);
                        /* double division to avoid underflow */
                        for (j = l; j < n; j++)
                        {
                            for (s = 0.0f, k = l; k < n; k++)
                                s += (a[i * n + k] * v[k * n + j]);
                            for (k = l; k < n; k++)
                                v[k * n + j] += (s * v[k * n + i]);
                        }
                    }
                    for (j = l; j < n; j++)
                        v[i * n + j] = v[j * n + i] = 0.0f;
                }
                v[i * n + i] = 1.0f;
                g = rv1[i];
                l = i;
            }

            /* accumulate the left-hand transformation */
            for (i = n - 1; i >= 0; i--)
            {
                l = i + 1;
                g = w[i];
                if (i < n - 1)
                    for (j = l; j < n; j++)
                        a[i * n + j] = 0.0f;
                if (g!=0)
                {
                    g = 1.0f / g;
                    if (i != n - 1)
                    {
                        for (j = l; j < n; j++)
                        {
                            for (s = 0.0f, k = l; k < m; k++)
                                s += (a[k * n + i] * a[k * n + j]);
                            f = (s / a[i * n + i]) * g;
                            for (k = i; k < m; k++)
                                a[k * n + j] += (f * a[k * n + i]);
                        }
                    }
                    for (j = i; j < m; j++)
                        a[j * n + i] = (a[j * n + i] * g);
                }
                else
                {
                    for (j = i; j < m; j++)
                        a[j * n + i] = 0.0f;
                }
                ++a[i * n + i];
            }

            /* diagonalize the bidiagonal form */
            for (k = n - 1; k >= 0; k--)
            {                             /* loop over singular values */
                for (its = 0; its < 30; its++)
                {                         /* loop over allowed iterations */
                    flag = 1;
                    for (l = k; l >= 0; l--)
                    {                     /* test for splitting */
                        nm = l - 1;
                        if (Math.Abs(rv1[l]) + anorm == anorm)
                        {
                            flag = 0;
                            break;
                        }
                        if (Math.Abs(w[nm]) + anorm == anorm)
                            break;
                    }
                    if (flag != 0)
                    {
                        c = 0.0f;
                        s = 1.0f;
                        for (i = l; i <= k; i++)
                        {
                            f = s * rv1[i];
                            if (Math.Abs(f) + anorm != anorm)
                            {
                                g = w[i];
                                h = PYTHAG(f, g);
                                w[i] = h;
                                h = 1.0f / h;
                                c = g * h;
                                s = (-f * h);
                                for (j = 0; j < m; j++)
                                {
                                    y = a[j * n + nm];
                                    z = a[j * n + i];
                                    a[j * n + nm] = (y * c + z * s);
                                    a[j * n + i] = (z * c - y * s);
                                }
                            }
                        }
                    }
                    z = w[k];
                    if (l == k)
                    {                  /* convergence */
                        if (z < 0.0f)
                        {              /* make singular value nonnegative */
                            w[k] = (-z);
                            for (j = 0; j < n; j++)
                                v[j * n + k] = (-v[j * n + k]);
                        }
                        break;
                    }
                    if (its >= 30)
                    {
                        return;
                    }

                    /* shift from bottom 2 x 2 minor */
                    x = w[l];
                    nm = k - 1;
                    y = w[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0f * h * y);
                    g = PYTHAG(f, 1.0f);
                    f = ((x - z) * (x + z) + h * ((y / (f + SIGN(g, f))) - h)) / x;

                    /* next QR transformation */
                    c = s = 1.0f;
                    for (j = l; j <= nm; j++)
                    {
                        i = j + 1;
                        g = rv1[i];
                        y = w[i];
                        h = s * g;
                        g = c * g;
                        z = PYTHAG(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y = y * c;
                        for (jj = 0; jj < n; jj++)
                        {
                            x = v[jj * n + j];
                            z = v[jj * n + i];
                            v[jj * n + j] = (x * c + z * s);
                            v[jj * n + i] = (z * c - x * s);
                        }
                        z = PYTHAG(f, h);
                        w[j] = z;
                        if (z!=0)
                        {
                            z = 1.0f / z;
                            c = f * z;
                            s = h * z;
                        }
                        f = (c * g) + (s * y);
                        x = (c * y) - (s * g);
                        for (jj = 0; jj < m; jj++)
                        {
                            y = a[jj * n + j];
                            z = a[jj * n + i];
                            a[jj * n + j] = (y * c + z * s);
                            a[jj * n + i] = (z * c - y * s);
                        }
                    }
                    rv1[l] = 0.0f;
                    rv1[k] = f;
                    w[k] = x;
                }
            }
            return;
        }


        private static double f(double[] param, double x, double y)
        {
            return (param[0] + param[1] * Math.Exp(-(Math.Pow(x - param[4], 2) / (2 * Math.Pow(param[2], 2)) + Math.Pow(y - param[5], 2) / (2 * Math.Pow(param[3], 2)))));
        }

        public static double[] fit(double[] param, double[] zdata)
        {
            int n_iter = 2000;

            // Constant
            const double eps = 0.001f;

            // Matrices, Vectors
            double[] J = new double[DATASIZE * PARAMSIZE];
            double[] H = new double[PARAMSIZE * PARAMSIZE];
            double[] H_lm = new double[PARAMSIZE * PARAMSIZE];
            double[] H_tmp = new double[PARAMSIZE * PARAMSIZE];

            double[] d = new double[DATASIZE];
            double[] d_lm = new double[DATASIZE];
            double[] tmpparam = new double[PARAMSIZE];
            double[] param_lm = new double[PARAMSIZE];

            double[] x_y_data = new double[98] { 0, 0, 0, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 6,
                                                 1, 0, 1, 1, 1, 2, 1, 3, 1, 4, 1, 5, 1, 6,
                                                 2, 0, 2, 1, 2, 2, 2, 3, 2, 4, 2, 5, 2, 6,
                                                 3, 0, 3, 1, 3, 2, 3, 3, 3, 4, 3, 5, 3, 6,
                                                 4, 0, 4, 1, 4, 2, 4, 3, 4, 4, 4, 5, 4, 6,
                                                 5, 0, 5, 1, 5, 2, 5, 3, 5, 4, 5, 5, 5, 6,
                                                 6, 0, 6, 1, 6, 2, 6, 3, 6, 4, 6, 5, 6, 6 };

            // doubles
            double error = 0, error_lm = 0;
            double tmp = 0, tmp2 = 0;
            double lam = 0.01f;

            // For iterations
            int i = 0, j = 0, k = 0, N = 0;
            int x = 0, y = 0;

            // Flag
            int isUpdateJ = 1;

            for (N = 0; N < n_iter; N++)
            {
                if (isUpdateJ == 1)
                {
                    // Compute Jacobian
                    for (i = 0; i < DATASIZE; i++)
                    {
                        x = i * 2;
                        y = i * 2 + 1;
                        for (j = 0; j < PARAMSIZE; j++)
                        {
                            tmp = f(param, x_y_data[x], x_y_data[y]);
                            for (k = 0; k < PARAMSIZE; k++)
                            {
                                if (k == j)
                                {
                                    tmpparam[k] = param[k] + eps;
                                }
                                else
                                {
                                    tmpparam[k] = param[k];
                                }
                            }
                            tmp2 = f(tmpparam, x_y_data[x], x_y_data[y]);
                            J[i * PARAMSIZE + j] = (tmp2 - tmp) / eps;
                        }
                    }
                    // Compute Hessian
                    for (i = 0; i < PARAMSIZE; i++)
                    {
                        for (j = 0; j < PARAMSIZE; j++)
                        {
                            tmp = 0;
                            for (k = 0; k < DATASIZE; k++)
                            {
                                tmp += J[k * PARAMSIZE + i] * J[k * PARAMSIZE + j];
                            }
                            H[i * PARAMSIZE + j] = tmp;
                        }
                    }
                    // For the first iteration, Compute distance and error
                    if (N == 0)
                    {
                        error = 0;
                        for (i = 0; i < DATASIZE; i++)
                        {
                            d[i] = zdata[i] - f(param, x_y_data[i * 2], x_y_data[i * 2 + 1]);
                            error += d[i] * d[i];
                        }
                    }
                }
                // Compute Levenberg-Marquardt Hessian
                for (i = 0; i < PARAMSIZE; i++)
                {
                    H[i * PARAMSIZE + i] += H[i * PARAMSIZE + i] * lam;
                }

                //Compute inverse of Hessian
                dsvd(H, tmpparam, H_tmp); // U, S, V

                for (i = 0; i < PARAMSIZE; i++)
                {
                    for (j = 0; j < PARAMSIZE; j++)
                    {
                        // V * inv(S)
                        if (tmpparam[j] == 0)
                        {
                            H_tmp[i * PARAMSIZE + j] = 0;
                        }
                        else
                        {
                            H_tmp[i * PARAMSIZE + j] /= tmpparam[j];
                        }
                    }
                }
                for (i = 0; i < PARAMSIZE; i++)
                {
                    for (j = 0; j < PARAMSIZE; j++)
                    {
                        // V * inv(S) * U'
                        tmp = 0;
                        for (k = 0; k < PARAMSIZE; k++)
                        {
                            tmp += H_tmp[i * PARAMSIZE + k] * H[j * PARAMSIZE + k];
                        }
                        H_lm[i * PARAMSIZE + j] = tmp;
                    }
                }

                //Compute J' * d and store into tmpparam
                for (i = 0; i < PARAMSIZE; i++)
                {
                    tmp = 0;
                    for (j = 0; j < DATASIZE; j++)
                    {
                        tmp += J[j * PARAMSIZE + i] * d[j];
                    }
                    tmpparam[i] = tmp;
                }
                //Compute inverted_H * tmpparam, and calculate new param
                for (i = 0; i < PARAMSIZE; i++)
                {
                    tmp = 0;
                    for (j = 0; j < PARAMSIZE; j++)
                    {
                        tmp += H_lm[i * PARAMSIZE + j] * tmpparam[j];
                    }
                    param_lm[i] = param[i] + tmp;
                }
                //Compute new error
                error_lm = 0;
                for (i = 0; i < DATASIZE; i++)
                {
                    d_lm[i] = zdata[i] - f(param_lm, x_y_data[i * 2], x_y_data[i * 2 + 1]);
                    error_lm += d_lm[i] * d_lm[i];
                }
                //If the new error is bigger than the original error, decrease lambda. If else, increase lambda.
                if (error_lm < error)
                {
                    lam /= 10;
                    for (i = 0; i < PARAMSIZE; i++)
                    {
                        param[i] = param_lm[i];
                    }
                    error = error_lm;
                    for (i = 0; i < DATASIZE; i++)
                    {
                        d[i] = d_lm[i];
                    }
                    isUpdateJ = 1;
                }
                else
                {
                    lam *= 10;
                    if (lam > 1e16f) break;
                    isUpdateJ = 0;
                }
                if (error < eps) break;
            }
            return new double[3] { param[5], param[4], error};
        }
    }
}

